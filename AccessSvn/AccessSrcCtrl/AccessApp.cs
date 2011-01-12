﻿using System;
using System.Collections.Generic;
using System.Text;
using Access = Microsoft.Office.Interop.Access;
using System.IO;

namespace AccessIO {

    /// <summary>
    /// Base class for both access file formats: mdb and adp
    /// </summary>
    /// <remarks>
    /// Allow to get access uniformly to the different object types of an access file
    /// </remarks>
    public abstract class AccessApp : IDisposable {

        /// <summary>
        /// Access.Application interop object
        /// </summary>
        public Access.Application Application { get; protected set; }

        /// <summary>
        /// Project type (mdb or adp)
        /// </summary>
        protected AccessProjectType ProjectType { get; set; }

        /// <summary>
        /// File name of the Access database/project file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// List of ObjectType object allowed depending on the file type: database or project
        /// </summary>
        public ObjectType[] AllowedObjetTypes {
            get;
            protected set;
        }

        /// <summary>
        /// Create a new instance of an AccessApp derived class, depending on the file name extension of <paramref name="fileName"/>
        /// </summary>
        /// <param name="fileName">File name of the Access database/project file</param>
        /// <returns>Returns an initialized AccessApp derived class</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability","CA2000:DisposeObjectsBeforeLosingScope")]
        public static AccessApp AccessAppFactory(string fileName) {
            AccessApp app = null;
            switch (Path.GetExtension(fileName).ToUpperInvariant()) { 
                case ".MDE":
                case ".MDB":
                    app = new AccessMdb(fileName);
                    break;
                case ".ADP":
                case ".ADE":
                    app = new AccessAdp(fileName);
                    break;
            }
            app.IntanceAccessApplication();
            app.InitializeAllowedObjetTypes();
            return app;

        }

        /// <summary>
        /// Creates a new instance of Access.Application interop object and loads the database or project file
        /// </summary>
        protected virtual void IntanceAccessApplication() {
            Application = new Access.Application();
            Application.UserControl = false;
            Application.Visible = false;
            if (ProjectType == AccessProjectType.Adp)
                Application.OpenAccessProject(FileName);
            else
                Application.OpenCurrentDatabase(FileName);
        }

        /// <summary>
        /// Initialize the allowed ObjectType depending on the file type: database or project
        /// </summary>
        protected abstract void InitializeAllowedObjetTypes();

        /// <summary>
        /// Returns a friendly contanier name for each Access object type
        /// </summary>
        /// <param name="objectType">Access object Type</param>
        /// <returns>string with the container name</returns>
        public static string GetContainerNameFromObjectType(ObjectType objectType) {
            switch (objectType) {
                case ObjectType.DatabaseDao:
                case ObjectType.DatabasePrj:
                    //return "Database";
                    return string.Empty;
                case ObjectType.DataAccessPage:
                    return "DataAccessPages";
                case ObjectType.Diagram:
                case ObjectType.Relations:
                    //return "Relations";
                    return String.Empty;
                case ObjectType.Form:
                    return "Forms";
                case ObjectType.Macro:
                    return "Scripts";
                case ObjectType.Module:
                    return "Modules";
                case ObjectType.Query:
                    return "Queries";
                case ObjectType.Report:
                    return "Reports";
                case ObjectType.Table:
                    return "Tables";
                case ObjectType.References:
                    //return "References";
                    return String.Empty;
                default:
                    throw new ArgumentException(Properties.Resources.NotAllowedObjectTypeException, "objectType");
            }
        }

        #region IDisposable Members

        private bool disposed = false;
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispoing) {
            if (!this.disposed) {
                if (dispoing) {
                    if (Application != null) {
                        Application.Quit();
                        Application = null;
                    }
                }
                disposed = true;
            }
        }

        #endregion

        /// <summary>
        /// Returns a list with the object names of a determined type
        /// </summary>
        /// <param name="objectType">Object type to query</param>
        public abstract List<IObjectName> LoadObjectNames(ObjectType objectType);

        /// <summary>
        /// Returns if a ObjectType is valid or not for the project type
        /// </summary>
        /// <param name="objectType">ObjectType to query</param>
        /// <returns><c>true</c> if is an allowed type, else returns <c>false</c></returns>
        protected virtual bool IsAllowedType(ObjectType objectType) {
            foreach (ObjectType ot in AllowedObjetTypes) {
                if (ot == objectType)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a list of <see cref="ObjectName"/> 
        /// </summary>
        /// <returns></returns>
        protected virtual List<ObjectName> GetReferences() {
            List<ObjectName> lst = new List<ObjectName>();
            foreach (Access.Reference reference in Application.References) {
                if (!reference.BuiltIn) {
                    lst.Add(new ObjectName(reference.Name, ObjectType.References));
                }
            }
            return lst;
        }

        /// <summary>
        /// Absolute or relative path to the base directory of svn working copy
        /// </summary>
        public string SvnBasePath { get; set; }

        /// <summary>
        /// Root local path for the working copy
        /// </summary>
        public string WorkingCopyPath { get; set; }
    }
}
