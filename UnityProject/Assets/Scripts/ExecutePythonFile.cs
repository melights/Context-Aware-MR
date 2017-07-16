using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Diagnostics;


public class ExecutePythonFile : MonoBehaviour {

    private string m_pythonExePath;
    private string m_filePath;
    private string m_directory;

    private List<string> m_arguments;
    private List<System.EventHandler> m_eventHandlers;


    public void Setup(
        string pythonExePath,
        string pythonFilePath,
        string pythonWorkingDir,
        List<string> args,
         List<System.EventHandler> handlers
        )
    {
        m_pythonExePath = pythonExePath;
        m_filePath = pythonFilePath;
        m_directory = pythonWorkingDir;
        m_arguments = args;
        m_eventHandlers = handlers;
    }

    public void Start()
    {
        ;
    }

    public void RunPython()
    {
        StartCoroutine("Execute");
    }

    private IEnumerator Execute()
    {
        Run(ref m_filePath,ref m_directory,ref m_pythonExePath, ref m_arguments, ref m_eventHandlers);
        yield return null;
    }

    // from:
    // http://onedayitwillmake.com/blog/2014/10/running-a-pythonshell-script-from-the-unityeditor/

    private void Run(ref string filepath,ref string wrkingDir,ref string pythonExePath, ref List<string> args, ref List<System.EventHandler> handlers)
    {
        Process p = new Process();

        string argsList = "";

        foreach (string s in args)
        {
            argsList += " ";
            argsList += s;
        }

        p.StartInfo = new ProcessStartInfo(pythonExePath, filepath + argsList)
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,

            // Where the script lives
            WorkingDirectory = wrkingDir,// Application.dataPath;
            UseShellExecute = false // must be false when redirecting IO
        };

        p.EnableRaisingEvents = true;

        // Add callbacks
        p.Exited += new System.EventHandler(ProcessExitCallback);
        for (int i = 0; i < handlers.Count; i++)
        {
            p.Exited += handlers[i];
        }

        p.Start();
    }

    private void ProcessExitCallback(object sender, System.EventArgs e)
    {
        UnityEngine.Debug.Log("Python Script Finished");
    }
}
