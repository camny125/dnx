﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Framework.Runtime.Compilation;

namespace Microsoft.Framework.Runtime.Compilation.DesignTime
{
    public class DesignTimeHostCompiler : IDesignTimeHostCompiler
    {
        private readonly ProcessingQueue _queue;
        private readonly IApplicationShutdown _shutdown;
        private readonly ConcurrentDictionary<int, TaskCompletionSource<CompileResponse>> _compileResponses = new ConcurrentDictionary<int, TaskCompletionSource<CompileResponse>>();
        private readonly TaskCompletionSource<Dictionary<string, int>> _projectContexts = new TaskCompletionSource<Dictionary<string, int>>();
        private readonly IFileWatcher _watcher;

        public DesignTimeHostCompiler(IApplicationShutdown shutdown, IFileWatcher watcher, Stream stream)
        {
            _shutdown = shutdown;
            _watcher = watcher;
            _queue = new ProcessingQueue(stream);
            _queue.ProjectCompiled += OnProjectCompiled;
            _queue.ProjectsInitialized += ProjectContextsInitialized;
            _queue.ProjectChanged += _ => { };
            _queue.ProjectSources += files =>
            {
                foreach (var file in files)
                {
                    watcher.WatchFile(file);
                }
            };
            _queue.Error += OnError;

            _queue.Closed += OnClosed;
            _queue.Start();

            _queue.Send(new EnumerateProjectContextsMessage());
        }

        private void OnError(int? contextId, string error)
        {
            var exception = new InvalidOperationException(error);
            if (contextId == null || contextId == -1)
            {
                _projectContexts.TrySetException(exception);
                _shutdown.RequestShutdown();
            }
            else
            {
                _compileResponses.AddOrUpdate(contextId.Value,
                _ =>
                {
                    var tcs = new TaskCompletionSource<CompileResponse>();
                    tcs.SetException(exception);
                    return tcs;
                },
                (_, existing) =>
                {
                    if (!existing.TrySetException(exception))
                    {
                        var tcs = new TaskCompletionSource<CompileResponse>();
                        tcs.TrySetException(exception);
                        return tcs;
                    }

                    return existing;
                });
            }
        }

        public async Task<CompileResponse> Compile(string projectPath, CompilationTarget library)
        {
            var contexts = await _projectContexts.Task.ConfigureAwait(false);

            int contextId;
            if (!contexts.TryGetValue(projectPath, out contextId))
            {
                throw new InvalidOperationException($"The context of the project at {projectPath} could not be determined. This can happen if a project references other projects by source, and those projects have a global.json file specifying a different version of the SDK.");
            }

            _queue.Send(new GetCompiledAssemblyMessage
            {
                Name = library.Name,
                Configuration = library.Configuration,
                TargetFramework = library.TargetFramework,
                Aspect = library.Aspect,
                ContextId = contextId
            });

            _watcher.WatchProject(projectPath);

            var task = _compileResponses.GetOrAdd(contextId, _ => new TaskCompletionSource<CompileResponse>()).Task;
            return await task.ConfigureAwait(false);
        }

        private void OnClosed()
        {
            // Cancel all pending responses
            foreach (var q in _compileResponses)
            {
                q.Value.TrySetCanceled();
            }
        }

        private void ProjectContextsInitialized(Dictionary<string, int> projectContexts)
        {
            _projectContexts.TrySetResult(projectContexts);
        }

        private void OnProjectCompiled(int contextId, CompileResponse response)
        {
            _compileResponses.AddOrUpdate(contextId,
                _ =>
                {
                    var tcs = new TaskCompletionSource<CompileResponse>();
                    tcs.SetResult(response);
                    return tcs;
                },
                (_, existing) =>
                {
                    if (!existing.TrySetResult(response))
                    {
                        var tcs = new TaskCompletionSource<CompileResponse>();
                        tcs.SetResult(response);
                        return tcs;
                    }

                    return existing;
                });
        }
    }
}
