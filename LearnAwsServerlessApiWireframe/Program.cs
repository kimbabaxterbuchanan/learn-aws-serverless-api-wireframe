using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LearnAwsServerlessApiWireframe
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new LearnAwsServerlessApiWireframeStack(app, "LearnAwsServerlessApiWireframeStack", new StackProps
            {
                Env = new Amazon.CDK.Environment
                {
                    Account = "",
                    Region = "",
                }

                // For more information, see https://docs.aws.amazon.com/cdk/latest/guide/environments.html
            });
            app.Synth();
        }
    }
}
