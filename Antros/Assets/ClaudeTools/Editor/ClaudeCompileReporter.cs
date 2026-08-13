using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace ClaudeTools
{
    /// <summary>
    /// Writes every script compilation result to <c>Logs/claude-compile.log</c> at the project root,
    /// so an assistant that can read the project folder (but not the Unity Editor UI) can verify that
    /// edits compile. It lives in its own reference-less asmdef, so it keeps compiling and reporting
    /// even when the project's other assemblies fail — which is exactly when the report matters.
    ///
    /// Nothing to run: Unity recompiles automatically when a script changes and focus returns to the
    /// Editor, and this rewrites the log each time. Delete the ClaudeTools folder to remove it.
    /// </summary>
    [InitializeOnLoad]
    public static class ClaudeCompileReporter
    {
        // reporter self-test trigger — safe to delete


        private static readonly string LogPath =
            Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Logs", "claude-compile.log");

        private static readonly StringBuilder Buffer = new();
        private static int errors;
        private static int warnings;

        static ClaudeCompileReporter()
        {
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
        }

        private static void OnCompilationStarted(object _)
        {
            Buffer.Clear();
            errors = 0;
            warnings = 0;
        }

        private static void OnAssemblyFinished(string assemblyPath, CompilerMessage[] messages)
        {
            string assembly = Path.GetFileNameWithoutExtension(assemblyPath);
            foreach (CompilerMessage m in messages)
            {
                bool isError = m.type == CompilerMessageType.Error;
                if (isError)
                    errors++;
                else
                    warnings++;

                Buffer.AppendLine(
                    $"{(isError ? "ERROR" : "warn ")} [{assembly}] {m.file}({m.line},{m.column}): {m.message}");
            }
        }

        private static void OnCompilationFinished(object _)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath));

                StringBuilder report = new();
                report.AppendLine($"# Claude compile report — {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                report.AppendLine($"RESULT: {(errors == 0 ? "OK" : "FAIL")}  ERRORS: {errors}  WARNINGS: {warnings}");
                report.AppendLine();
                report.Append(Buffer.Length > 0 ? Buffer.ToString() : "(no messages)");

                File.WriteAllText(LogPath, report.ToString());
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[ClaudeCompileReporter] Couldn't write {LogPath}: {e.Message}");
            }
        }
    }
}
