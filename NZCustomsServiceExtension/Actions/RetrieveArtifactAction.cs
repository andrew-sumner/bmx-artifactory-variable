using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Web;
using NZCustomsServiceExtension.Variables;
using System.Threading;

namespace NZCustomsServiceExtension.Actions
{
    [ActionProperties(
    "Retrieve Artifact",
    "Retrieves an atifact from a repository (Supports SSH).")]
    [Tag("NZCustomsService")]
    [CustomEditor(typeof(RetrieveArtifactActionEditor))]
    public class RetrieveArtifactAction : AgentBasedActionBase
    {
        [Persistent]
        public string ItemName { get; set; }

        [Persistent]
        public string FileName { get; set; }

        public RetrieveArtifactAction()
        {
        }

        public override string ToString()
        {
            return string.Format("Retrieve the {0} artifact {1}", this.ItemName, this.FileName);
        }

        protected override void Execute()
        {
            this.LogInformation("ProcessRemoteCommand");
            string fname = this.ResolveDirectory(this.FileName);

            this.LogInformation("fname=" + fname);
            string onlyFileName = Path.GetFileName(fname);
            this.LogInformation("onlyFileName=" + onlyFileName);
            StringBuilder url = new StringBuilder();

            ArtifactoryConfigurer config = this.GetExtensionConfigurer() as ArtifactoryConfigurer;
            this.LogInformation("config=" + config.Server);
            string server = config.Server;


            url.Append(server.EndsWith("/") ? server : server + "/");
            url.Append(this.ItemName);

            DownloadFile(url.ToString(), "/tmp/buildmaster/play.txt");
        }

        protected string ResolveDirectory(string FilePath)
        {
            this.LogInformation("ResolveDirectory=" + FilePath);
            var fileOps = this.Context.Agent.GetService<IFileOperationsExecuter>();
            var absWorkingDirectory = fileOps.GetWorkingDirectory(this.Context.ApplicationId, this.Context.DeployableId ?? 0, FilePath);
            this.LogInformation("absWorkingDirectory=" + absWorkingDirectory);

            return "/tmp/buildmaster/art.txt";

            //using (var sourceAgent2 = Util.Agents.CreateLocalAgent())
            //{
            //    var sourceAgent = sourceAgent2.GetService<IFileOperationsExecuter>();

            //    char srcSeparator = sourceAgent.GetDirectorySeparator();
            //    var srcPath = sourceAgent.GetWorkingDirectory(this.Context.ApplicationId, this.Context.DeployableId ?? 0, FilePath);

            //    LogInformation("Source Path: " + srcPath);
            //    return srcPath;
            //}
        }

        //TODO: If this happens to work, replace with an ASYNC method so incrementally download and write to ssh server
        // block by block.  Test with a large file and expect immediately start writing to ssh rather than long delay
        // and then blat it out.
        public int DownloadFile(String remoteFilename, String sshFilename)
        {
            // Function will return the number of bytes processed
            // to the caller. Initialize to 0 here.
            int bytesProcessed = 0;

            // Assign values to these objects here so that they can
            // be referenced in the finally block
            Stream remoteStream = null;
            Stream sshStream = null;
            WebResponse response = null;

            var sshFileOps = this.Context.Agent.GetService<IFileOperationsExecuter>();
            
            // Use a try/catch/finally block as both the WebRequest and Stream
            // classes throw exceptions upon error
            try
            {
                // Create a request for the specified remote file name
                WebRequest request = WebRequest.Create(remoteFilename);
                request.Credentials = new NetworkCredential("admin", "password");
                
                // Send the request to the server and retrieve the
                // WebResponse object 
                response = request.GetResponse();
                if (response != null)
                {
                    // Once the WebResponse object has been retrieved,
                    // get the stream object associated with the response's data
                    remoteStream = response.GetResponseStream();

                    // Create the local file
                    sshStream = sshFileOps.OpenFile(sshFilename, FileMode.Create, FileAccess.Write);

                    // Allocate a 1k buffer
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    // Simple do/while loop to read from stream until
                    // no bytes are returned
                    do
                    {
                        // Not sure that this isn't blocking
                        Thread.Yield();

                        // Read data (up to 1k) from the stream
                        bytesRead = remoteStream.Read(buffer, 0, buffer.Length);

                        // Write the data to the SSH file
                        sshStream.Write(buffer, 0, bytesRead);

                        // Increment total bytes processed
                        bytesProcessed += bytesRead;

                        this.LogDebug("{0} bytes downloaded", bytesProcessed);

                    } while (bytesRead > 0);
                }                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // Close the response and streams objects here 
                // to make sure they're closed even if an exception
                // is thrown at some point
                if (response != null) response.Close();
                if (remoteStream != null) remoteStream.Close();
                if (sshStream != null) sshStream.Close();
            }

            // Return total bytes processed to caller.
            return bytesProcessed;
        }
    }

    
}