﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Cowboy.Extensions;
using Cowboy.Routing;

namespace Cowboy
{
    public abstract class Module : IHideObjectMembers
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Regex ModuleNameExpression = new Regex(@"(?<name>[\w]+)Module$", RegexOptions.Compiled);

        private readonly List<Route> routes = new List<Route>();

        protected Module()
            : this(string.Empty)
        {
        }

        protected Module(string modulePath)
        {
            this.ModulePath = modulePath;
        }

        public string ModulePath { get; protected set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual IEnumerable<Route> Routes
        {
            get { return this.routes.AsReadOnly(); }
        }

        public string GetModuleName()
        {
            var typeName = this.GetType().Name;
            var nameMatch = ModuleNameExpression.Match(typeName);

            if (nameMatch.Success)
            {
                return nameMatch.Groups["name"].Value;
            }

            return typeName;
        }

        //public dynamic ViewBag
        //{
        //    get
        //    {
        //        return this.Context == null ? null : this.Context.ViewBag;
        //    }
        //}

        //public dynamic Text
        //{
        //    get { return this.Context.Text; }
        //}

        //public ISession Session
        //{
        //    get { return this.Request.Session; }
        //}

        //public ViewRenderer View
        //{
        //    get { return new ViewRenderer(this); }
        //}

        //public Negotiator Negotiate
        //{
        //    get { return new Negotiator(this.Context); }
        //}

        public virtual Request Request
        {
            get { return this.Context.Request; }
            set { this.Context.Request = value; }
        }

        public Context Context { get; set; }

        public ResponseFormatter Response { get; set; }

        public RouteBuilder Delete
        {
            get { return new RouteBuilder("DELETE", this); }
        }

        public RouteBuilder Get
        {
            get { return new RouteBuilder("GET", this); }
        }

        public RouteBuilder Head
        {
            get { return new RouteBuilder("HEAD", this); }
        }

        public RouteBuilder Options
        {
            get { return new RouteBuilder("OPTIONS", this); }
        }

        public RouteBuilder Patch
        {
            get { return new RouteBuilder("PATCH", this); }
        }

        public RouteBuilder Post
        {
            get { return new RouteBuilder("POST", this); }
        }

        public RouteBuilder Put
        {
            get { return new RouteBuilder("PUT", this); }
        }

        public class RouteBuilder : IHideObjectMembers
        {
            private readonly string method;
            private readonly Module parentModule;

            public RouteBuilder(string method, Module parentModule)
            {
                this.method = method;
                this.parentModule = parentModule;
            }

            public Func<dynamic, dynamic> this[string path]
            {
                set { this.AddRoute(string.Empty, path, null, value); }
            }

            public Func<dynamic, dynamic> this[string path, Func<Context, bool> condition]
            {
                set { this.AddRoute(string.Empty, path, condition, value); }
            }

            public Func<dynamic, CancellationToken, Task<dynamic>> this[string path, bool runAsync]
            {
                set { this.AddRoute(string.Empty, path, null, value); }
            }

            public Func<dynamic, CancellationToken, Task<dynamic>> this[string path, Func<Context, bool> condition, bool runAsync]
            {
                set { this.AddRoute(string.Empty, path, condition, value); }
            }

            public Func<dynamic, dynamic> this[string name, string path]
            {
                set { this.AddRoute(name, path, null, value); }
            }

            public Func<dynamic, dynamic> this[string name, string path, Func<Context, bool> condition]
            {
                set { this.AddRoute(name, path, condition, value); }
            }

            public Func<dynamic, CancellationToken, Task<dynamic>> this[string name, string path, bool runAsync]
            {
                set { this.AddRoute(name, path, null, value); }
            }

            public Func<dynamic, CancellationToken, Task<dynamic>> this[string name, string path, Func<Context, bool> condition, bool runAsync]
            {
                set { this.AddRoute(name, path, condition, value); }
            }

            protected void AddRoute(string name, string path, Func<Context, bool> condition, Func<dynamic, dynamic> value)
            {
                var fullPath = GetFullPath(path);

                this.parentModule.routes.Add(Route.FromSync(name, this.method, fullPath, condition, value));
            }

            protected void AddRoute(string name, string path, Func<Context, bool> condition, Func<dynamic, CancellationToken, Task<dynamic>> value)
            {
                var fullPath = GetFullPath(path);

                this.parentModule.routes.Add(new Route(name, this.method, fullPath, condition, value));
            }

            private string GetFullPath(string path)
            {
                var relativePath = (path ?? string.Empty).Trim('/');
                var parentPath = (this.parentModule.ModulePath ?? string.Empty).Trim('/');

                if (string.IsNullOrEmpty(parentPath))
                {
                    return string.Concat("/", relativePath);
                }

                if (string.IsNullOrEmpty(relativePath))
                {
                    return string.Concat("/", parentPath);
                }

                return string.Concat("/", parentPath, "/", relativePath);
            }
        }
    }
}