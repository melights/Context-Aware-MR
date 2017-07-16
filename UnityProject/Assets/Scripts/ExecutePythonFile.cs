using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Diagnostics;


public class ExecutePythonFile : MonoBehaviour {

    public string m_pythonExePath;
    public string m_filePath;
    public string m_directory;

    public List<string> m_arguments;

    // Use this for initialization
    void Start() {
        // for now
        StartPython();
    }

    public void StartPython()
    {
        StartCoroutine("Execute");
    }

    IEnumerator Execute()
    {
        Run(m_filePath, m_directory, m_pythonExePath);
        yield return null;
    }

    // from:
    // http://onedayitwillmake.com/blog/2014/10/running-a-pythonshell-script-from-the-unityeditor/

    private void Run(string filepath, string wrkingDir, string pythonExePath)
    {
        Process p = new Process();

        p.StartInfo = new ProcessStartInfo(pythonExePath, filepath)
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
