using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Diagnostics;


public class ExecutePythonFile : MonoBehaviour {

    private string m_pythonExePath;
    private string m_filePath;
    private string m_directory;

    private List<string> m_arguments;

    public void Setup(
        string pythonExePath,
        string pythonFilePath,
        string pythonWorkingDir,
        List<string> args
        )
    {
        m_pythonExePath = pythonExePath;
        m_filePath = pythonFilePath;
        m_directory = pythonWorkingDir;
        m_arguments = args;
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
        Run(m_filePath, m_directory, m_pythonExePath, m_arguments);
        yield return null;
    }

    // from:
    // http://onedayitwillmake.com/blog/2014/10/running-a-pythonshell-script-from-the-unityeditor/

    private void Run(string filepath, string wrkingDir, string pythonExePath, List<string> args)
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
        p.Exited += new System.EventHandler(ProcessExitCallback);
        p.Start();
    }

    private void ProcessExitCallback(object sender, System.EventArgs e)
    {
        UnityEngine.Debug.Log("Python Script Finished");
    }
}
