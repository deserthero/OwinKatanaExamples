using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace OwinKatanaExample1
{
    #region Description
    // Required Packages:
    // To put together a simple self-hosted web application:
    // 1- Install-Package Microsoft.Owin.Hosting
    // 2- Install-Package Microsoft.Owin.Host.HttpListener (we need to add a means for our console application to listen for HTTP requests)

    // Logic:
    // We have created a self-hosted web application using only a console application, and a handful of small Katana components.

    //Target: 
    // To know how basic middleware are constructed,
    // and how the middleware pipeline works in general.

    // How to run:
    // run project > type: http://localhost:8080 in the Browser.

    #endregion

    // use an alias for the OWIN AppFunc:
    using AppFunc = Func<IDictionary<string, object>, Task>;

    #region Console Application as a server 
    class Program
    {
        static void Main(string[] args)
        {
            Microsoft.Owin.Hosting.WebApp.Start<Startup>("http://localhost:8080");
            Console.WriteLine("Server Started; Press enter to Quit");
            Console.ReadLine();
        }

    }
    #endregion

    #region Startup Class
    /// <summary>
    /// In the Katana implementation of the OWIN specification, the host will look for a startup entry point to build
    ///  the middleware pipeline in one of four ways (in order as listed below):
    //    1- The Startup class is specified as a command line argument, or a type argument(where applicable) when the host in initialized(usually when using OwinHost, or the Owin.Hosting API, which is what we did in our code above).
    //    2- The host will look in the relevant app.Config or web.Config file for an appSettings entry with the key “owin:AppStartup”
    //    3- The host will scan the loaded assemblies for the OwinStartup attribute and uses the type specified in the attribute.
    //    4- If all of the preceding methods fail, then the host will use reflection and scan the loaded assemblies for a type named Startup with a method with the name and signature void Configuration(IAppBuilder).
    /// </summary>
    public class Startup
    {

        /// <summary>
        /// The Startup class must provide a public Configuration() method,
        ///  with the signature void Configure(IAppBuilder app).
        /// </summary>
        /// <param name="app">The IAppBuilder interface is NOT a part of the OWIN specification. It is, however,
        ///  a required component for a Katana host. The IAppBuilder interface provides a core set of methods required to
        ///  implement the OWIN standard, and serves as a base for additional extension methods for implementing middleware.
        /// When the Katana host initializes the Startup class and calls Configuration(), a concrete instance of IAppBuilder is passed as the argument.
        ///  We then use IAppBuilder to configure and add the application middleware components we need for our application,
        ///  assembling the pipeline through which incoming HTTP requests will be processed.
        /// </param>
        public void Configuration(Owin.IAppBuilder app)
        {

            //The most common way to add middleware is by passing components to the Use() method.
            //Middleware components will be added to the pipeline in the order they are passed to Use().
            //This is important to bear in mind as we configure our pipeline,
            //as this will determine the order in which processing is applied to incoming requests (and in reverse, to outgoing responses).

            //In our code, we grab a reference to our middleware function by calling MyMiddleware(),
            // and then add it to the pipeline be passing it to app.Use().

            var middleware = new Func<AppFunc, AppFunc>(MyMiddleWare);
            app.Use(middleware);
        }

        /// <summary>
        /// Bear in mind that the MyMiddleware() method simply returns the anonymous function
        ///  to the caller, but does not invoke it.
        ///  The function will be added the to request processing pipeline, 
        /// and will be invoked when an incoming HTTP request needs to be processed.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public AppFunc MyMiddleWare(AppFunc next)
        {
            AppFunc appFunc = async (IDictionary<string, object> environment) =>
            {
                // Do something with the incoming request:
                var response = environment["owin.ResponseBody"] as Stream;
                using (var writer = new StreamWriter(response))
                {
                    await writer.WriteAsync("<h1>Hello from My First Middleware</h1>");
                }
                // Call the next Middleware in the chain:
                await next.Invoke(environment);
            };
            return appFunc;
        }
    }
    #endregion
}
