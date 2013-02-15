/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Runtime.InteropServices;
using Microsoft.NodeTools.Project;
using Microsoft.PythonTools.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.NodeTools {
    [Guid("9092AA53-FB77-4645-B42D-1CCCA6BD08BD")]
    class BaseNodeProjectFactory : ProjectFactory {
#if FALSE
        [ThreadStatic]
        internal static bool? CreatingNew;
#endif

        public BaseNodeProjectFactory(NodeProjectPackage package) : base(package) {
        }

        protected override ProjectNode CreateProject() {
            NodejsProjectNode project = new NodejsProjectNode((NodeProjectPackage)Package);
            project.SetSite((IOleServiceProvider)((IServiceProvider)Package).GetService(typeof(IOleServiceProvider)));
            return project;
        }

#if FALSE
        protected override object PreCreateForOuter(IntPtr outerProjectIUnknown) {
            return new ProjectWrapper();
        }

        protected override void CreateProject(string fileName, string location, string name, uint flags, ref Guid projectGuid, out IntPtr project, out int canceled) {
            project = IntPtr.Zero;
            canceled = 0;

            // Get the list of GUIDs from the project/template
            string guidsList = this.ProjectTypeGuids(fileName);

            // Launch the aggregate creation process (we should be called back on our IVsAggregatableProjectFactoryCorrected implementation)
            IVsCreateAggregateProject aggregateProjectFactory = (IVsCreateAggregateProject)NodePackage.GetGlobalService(typeof(SVsCreateAggregateProject));
            CreatingNew = ((flags & (uint)__VSCREATEPROJFLAGS.CPF_CLONEFILE) != 0);
            try {
                int hr = aggregateProjectFactory.CreateAggregateProject(guidsList, fileName, location, name, flags & ~(uint)__VSCREATEPROJFLAGS.CPF_CLONEFILE, ref projectGuid, out project);
                if (hr == VSConstants.E_ABORT)
                    canceled = 1;
                ErrorHandler.ThrowOnFailure(hr);
            } finally {
                CreatingNew = null;
            }
        }

        /// <summary>
        /// Retrives the list of project guids from the project file.
        /// If you don't want your project to be flavorable, override
        /// to only return your project factory Guid:
        ///      return this.GetType().GUID.ToString("B");
        /// </summary>
        /// <param name="file">Project file to look into to find the Guid list</param>
        /// <returns>List of semi-colon separated GUIDs</returns>
        protected override string ProjectTypeGuids(string file) {
            // Load the project so we can extract the list of GUIDs
            return "{3AF33F2E-1136-4D97-BBB7-1795711AC8B8};{349c5851-65df-11da-9384-00065b846f21};{9092AA53-FB77-4645-B42D-1CCCA6BD08BD}";
            /*this.buildProject = Utilities.ReinitializeMsBuildProject(this.buildEngine, file, this.buildProject);

            // Retrieve the list of GUIDs, if it is not specify, make it our GUID
            string guids = buildProject.GetPropertyValue(ProjectFileConstants.ProjectTypeGuids);
            if (String.IsNullOrEmpty(guids))
                guids = this.GetType().GUID.ToString("B");

            return guids;*/
        }
#endif
    }

#if FALSE
    class ProjectWrapper :
        IVsGetCfgProvider,
        IVsProject4,
        IPersistFileFormat,
        IVsProjectBuildSystem,
        IVsBuildPropertyStorage,
        IVsSccProject2,
        IVsProjectSpecialFiles,
        IVsProjectUpgrade,
        IVsUpdateSolutionEvents,
        INewProjectInitializationProvider,
        IVsBrowseObjectContext,
        IVsProjectFlavorReferences,
        IHostObjectProvider,
        IVsFileBackup,
        //IProjectGuidService,
        IVsProjectSpecificEditorMap,
        //IVsProjectTreeService,
        IVsProjectFaultResolver,
        IVsManifestReferenceResolver,
        IVsUIHierarchy,
        IVsPersistHierarchyItem2,
        Microsoft.VisualStudio.OLE.Interop.IOleCommandTarget,
        IVsHierarchyDeleteHandler,
        IVsHierarchyDeleteHandler3,
        IVsAggregatableProjectCorrected,
    IVsAggregatableProject{
        internal object _inner;
        private string _aggregateProjectTypes;

        public ProjectWrapper() {
        }

        public ProjectWrapper(object inner) {
            _inner = inner;
        }

        public int GetAggregateProjectTypeGuids(out string pbstrProjTypeGuids) {
            pbstrProjTypeGuids = "{3AF33F2E-1136-4D97-BBB7-1795711AC8B8};{349c5851-65df-11da-9384-00065b846f21};{9092AA53-FB77-4645-B42D-1CCCA6BD08BD}";
            return VSConstants.S_OK;
        }

        public int InitializeForOuter(string pszFilename, string pszLocation, string pszName, uint grfCreateFlags, ref Guid iidProject, out IntPtr ppvProject, out int pfCanceled) {
            var sln = (IVsSolution)NodePackage.GetGlobalService(typeof(SVsSolution));

            IVsProjectFactory curFact;
            var guid = Guid.Parse("262852c6-cd72-467d-83fe-5eeb1973a190");
            ErrorHandler.ThrowOnFailure(
                sln.GetProjectFactory(
                    0,
                    new[] { guid },
                    pszFilename,
                    out curFact
                )
            );

            IntPtr innerProj;
            if (BaseNodeProjectFactory.CreatingNew.HasValue && BaseNodeProjectFactory.CreatingNew.Value) {
                grfCreateFlags |= (uint)__VSCREATEPROJFLAGS.CPF_CLONEFILE;
            }

            ErrorHandler.ThrowOnFailure(curFact.CreateProject(
                pszFilename, pszLocation, pszName, grfCreateFlags, ref iidProject, out innerProj, out pfCanceled));

            if (pfCanceled == 0) {
                _inner = Marshal.GetObjectForIUnknown(innerProj);

                pfCanceled = 0;
                return Marshal.QueryInterface(Marshal.GetIUnknownForObject(this), ref iidProject, out ppvProject);
            } else {
                ppvProject = IntPtr.Zero;
                return VSConstants.S_OK;
            }
        }

        public int OnAggregationComplete() {
            return VSConstants.S_OK;
        }

        public int SetAggregateProjectTypeGuids(string lpstrProjTypeGuids) {
            _aggregateProjectTypes = lpstrProjTypeGuids;
            return VSConstants.S_OK;
        }

        public int SetInnerProject(IntPtr punkInnerIUnknown) {
            return VSConstants.S_OK;
        }

        #region IVsGetCfgProvider Members

        int IVsGetCfgProvider.GetCfgProvider(out IVsCfgProvider ppCfgProvider) {
            return ((IVsGetCfgProvider)_inner).GetCfgProvider(out ppCfgProvider);
        }

        #endregion

        #region IVsProject4 Members

        public int AddItem(uint itemidLoc, VSADDITEMOPERATION dwAddItemOperation, string pszItemName, uint cFilesToOpen, string[] rgpszFilesToOpen, IntPtr hwndDlgOwner, VSADDRESULT[] pResult) {
            return ((IVsProject2)_inner).AddItem(itemidLoc, dwAddItemOperation, pszItemName, cFilesToOpen, rgpszFilesToOpen, hwndDlgOwner, pResult);
        }

        public int AddItemWithSpecific(uint itemidLoc, VSADDITEMOPERATION dwAddItemOperation, string pszItemName, uint cFilesToOpen, string[] rgpszFilesToOpen, IntPtr hwndDlgOwner, uint grfEditorFlags, ref Guid rguidEditorType, string pszPhysicalView, ref Guid rguidLogicalView, VSADDRESULT[] pResult) {
            return ((IVsProject3)_inner).AddItemWithSpecific(itemidLoc, dwAddItemOperation, pszItemName, cFilesToOpen, rgpszFilesToOpen, hwndDlgOwner, grfEditorFlags, ref rguidEditorType, pszPhysicalView, ref rguidLogicalView, pResult);
        }

        int IVsProject4.ContainsFileEndingWith(string pszEndingWith, out int pfDoesContain) {
            return ((IVsProject4)_inner).ContainsFileEndingWith(pszEndingWith, out pfDoesContain);
        }

        int IVsProject4.ContainsFileWithItemType(string pszItemType, out int pfDoesContain) {
            return ((IVsProject4)_inner).ContainsFileWithItemType(pszItemType, out pfDoesContain);
        }

        public int GenerateUniqueItemName(uint itemidLoc, string pszExt, string pszSuggestedRoot, out string pbstrItemName) {
            return ((IVsProject)_inner).GenerateUniqueItemName(itemidLoc, pszExt, pszSuggestedRoot, out pbstrItemName);
        }

        int IVsProject4.GetFilesEndingWith(string pszEndingWith, uint celt, uint[] rgItemids, out uint pcActual) {
            return ((IVsProject4)_inner).GetFilesEndingWith(pszEndingWith, celt, rgItemids, out pcActual);
        }

        int IVsProject4.GetFilesWithItemType(string pszItemType, uint celt, uint[] rgItemids, out uint pcActual) {
            return ((IVsProject4)_inner).GetFilesWithItemType(pszItemType, celt, rgItemids, out pcActual);
        }

        public int GetItemContext(uint itemid, out VisualStudio.OLE.Interop.IServiceProvider ppSP) {
            return ((IVsProject)_inner).GetItemContext(itemid, out ppSP);
        }

        public int GetMkDocument(uint itemid, out string pbstrMkDocument) {
            return ((IVsProject)_inner).GetMkDocument(itemid, out pbstrMkDocument);
        }

        public int IsDocumentInProject(string pszMkDocument, out int pfFound, VSDOCUMENTPRIORITY[] pdwPriority, out uint pitemid) {
            return ((IVsProject)_inner).IsDocumentInProject(pszMkDocument, out pfFound, pdwPriority, out pitemid);
        }

        public int OpenItem(uint itemid, ref Guid rguidLogicalView, IntPtr punkDocDataExisting, out IVsWindowFrame ppWindowFrame) {
            return ((IVsProject)_inner).OpenItem(itemid, ref rguidLogicalView, punkDocDataExisting, out ppWindowFrame);
        }

        public int OpenItemWithSpecific(uint itemid, uint grfEditorFlags, ref Guid rguidEditorType, string pszPhysicalView, ref Guid rguidLogicalView, IntPtr punkDocDataExisting, out IVsWindowFrame ppWindowFrame) {
            return ((IVsProject3)_inner).OpenItemWithSpecific(itemid, grfEditorFlags, ref rguidLogicalView, pszPhysicalView, ref rguidLogicalView, punkDocDataExisting, out ppWindowFrame);
        }

        public int RemoveItem(uint dwReserved, uint itemid, out int pfResult) {
            return ((IVsProject2)_inner).RemoveItem(dwReserved, itemid, out pfResult);
        }

        public int ReopenItem(uint itemid, ref Guid rguidEditorType, string pszPhysicalView, ref Guid rguidLogicalView, IntPtr punkDocDataExisting, out IVsWindowFrame ppWindowFrame) {
            return ((IVsProject2)_inner).ReopenItem(itemid, ref rguidLogicalView, pszPhysicalView, ref rguidLogicalView, punkDocDataExisting, out ppWindowFrame);
        }

        public int TransferItem(string pszMkDocumentOld, string pszMkDocumentNew, IVsWindowFrame punkWindowFrame) {
            return ((IVsProject3)_inner).TransferItem(pszMkDocumentNew, pszMkDocumentNew, punkWindowFrame);
        }

        #endregion

        #region IPersistFileFormat Members

        int IPersistFileFormat.GetClassID(out Guid pClassID) {
            return ((IPersistFileFormat)_inner).GetClassID(out pClassID);
        }

        int IPersistFileFormat.GetCurFile(out string ppszFilename, out uint pnFormatIndex) {
            return ((IPersistFileFormat)_inner).GetCurFile(out ppszFilename, out pnFormatIndex);
        }

        int IPersistFileFormat.GetFormatList(out string ppszFormatList) {
            return ((IPersistFileFormat)_inner).GetFormatList(out ppszFormatList);
        }

        int IPersistFileFormat.InitNew(uint nFormatIndex) {
            return ((IPersistFileFormat)_inner).InitNew(nFormatIndex);
        }

        int IPersistFileFormat.IsDirty(out int pfIsDirty) {
            return ((IPersistFileFormat)_inner).IsDirty(out pfIsDirty);
        }

        int IPersistFileFormat.Load(string pszFilename, uint grfMode, int fReadOnly) {
            return ((IPersistFileFormat)_inner).Load(pszFilename, grfMode, fReadOnly);
        }

        int IPersistFileFormat.Save(string pszFilename, int fRemember, uint nFormatIndex) {
            return ((IPersistFileFormat)_inner).Save(pszFilename, fRemember, nFormatIndex);
        }

        int IPersistFileFormat.SaveCompleted(string pszFilename) {
            return ((IPersistFileFormat)_inner).SaveCompleted(pszFilename);
        }

        #endregion

        #region IPersist Members

        int VisualStudio.OLE.Interop.IPersist.GetClassID(out Guid pClassID) {
            return ((VisualStudio.OLE.Interop.IPersist)_inner).GetClassID(out pClassID);
        }

        #endregion

        #region IVsProjectBuildSystem Members

        int IVsProjectBuildSystem.BuildTarget(string pszTargetName, out bool pbSuccess) {
            return ((IVsProjectBuildSystem)_inner).BuildTarget(pszTargetName, out pbSuccess);
        }

        int IVsProjectBuildSystem.CancelBatchEdit() {
            return ((IVsProjectBuildSystem)_inner).CancelBatchEdit();
        }

        int IVsProjectBuildSystem.EndBatchEdit() {
            return ((IVsProjectBuildSystem)_inner).EndBatchEdit();
        }

        int IVsProjectBuildSystem.GetBuildSystemKind(out uint pBuildSystemKind) {
            return ((IVsProjectBuildSystem)_inner).GetBuildSystemKind(out pBuildSystemKind);
        }

        int IVsProjectBuildSystem.SetHostObject(string pszTargetName, string pszTaskName, object punkHostObject) {
            return ((IVsProjectBuildSystem)_inner).SetHostObject(pszTargetName, pszTaskName, punkHostObject);
        }

        int IVsProjectBuildSystem.StartBatchEdit() {
            return ((IVsProjectBuildSystem)_inner).StartBatchEdit();
        }

        #endregion

        #region IVsBuildPropertyStorage Members

        int IVsBuildPropertyStorage.GetItemAttribute(uint item, string pszAttributeName, out string pbstrAttributeValue) {
            return ((IVsBuildPropertyStorage)_inner).GetItemAttribute(item, pszAttributeName, out pbstrAttributeValue);
        }

        int IVsBuildPropertyStorage.GetPropertyValue(string pszPropName, string pszConfigName, uint storage, out string pbstrPropValue) {
            return ((IVsBuildPropertyStorage)_inner).GetPropertyValue(pszPropName, pszConfigName, storage, out pbstrPropValue);
        }

        int IVsBuildPropertyStorage.RemoveProperty(string pszPropName, string pszConfigName, uint storage) {
            return ((IVsBuildPropertyStorage)_inner).RemoveProperty(pszPropName, pszConfigName, storage);
        }

        int IVsBuildPropertyStorage.SetItemAttribute(uint item, string pszAttributeName, string pszAttributeValue) {
            return ((IVsBuildPropertyStorage)_inner).SetItemAttribute(item, pszAttributeName, pszAttributeValue);
        }

        int IVsBuildPropertyStorage.SetPropertyValue(string pszPropName, string pszConfigName, uint storage, string pszPropValue) {
            return ((IVsBuildPropertyStorage)_inner).SetPropertyValue(pszPropName, pszConfigName, storage, pszPropValue);
        }

        #endregion

        #region IVsSccProject2 Members

        int IVsSccProject2.GetSccFiles(uint itemid, VisualStudio.OLE.Interop.CALPOLESTR[] pCaStringsOut, VisualStudio.OLE.Interop.CADWORD[] pCaFlagsOut) {
            return ((IVsSccProject2)_inner).GetSccFiles(itemid, pCaStringsOut, pCaFlagsOut);
        }

        int IVsSccProject2.GetSccSpecialFiles(uint itemid, string pszSccFile, VisualStudio.OLE.Interop.CALPOLESTR[] pCaStringsOut, VisualStudio.OLE.Interop.CADWORD[] pCaFlagsOut) {
            return ((IVsSccProject2)_inner).GetSccSpecialFiles(itemid, pszSccFile, pCaStringsOut, pCaFlagsOut);
        }

        int IVsSccProject2.SccGlyphChanged(int cAffectedNodes, uint[] rgitemidAffectedNodes, VsStateIcon[] rgsiNewGlyphs, uint[] rgdwNewSccStatus) {
            return ((IVsSccProject2)_inner).SccGlyphChanged(cAffectedNodes, rgitemidAffectedNodes, rgsiNewGlyphs, rgdwNewSccStatus);
        }

        int IVsSccProject2.SetSccLocation(string pszSccProjectName, string pszSccAuxPath, string pszSccLocalPath, string pszSccProvider) {
            return ((IVsSccProject2)_inner).SetSccLocation(pszSccProjectName, pszSccAuxPath, pszSccLocalPath, pszSccProvider);
        }

        #endregion

        #region IVsProjectSpecialFiles Members

        int IVsProjectSpecialFiles.GetFile(int fileID, uint grfFlags, out uint pitemid, out string pbstrFilename) {
            return ((IVsProjectSpecialFiles)_inner).GetFile(fileID, grfFlags, out pitemid, out pbstrFilename);
        }

        #endregion

        #region IVsProjectUpgrade Members

        int IVsProjectUpgrade.UpgradeProject(uint grfUpgradeFlags) {
            return ((IVsProjectUpgrade)_inner).UpgradeProject(grfUpgradeFlags);
        }

        #endregion

        #region IVsUpdateSolutionEvents Members

        int IVsUpdateSolutionEvents.OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy) {
            return ((IVsUpdateSolutionEvents)_inner).OnActiveProjectCfgChange(pIVsHierarchy);
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Begin(ref int pfCancelUpdate) {
            return ((IVsUpdateSolutionEvents)_inner).UpdateSolution_Begin(ref pfCancelUpdate);
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Cancel() {
            return ((IVsUpdateSolutionEvents)_inner).UpdateSolution_Cancel();
        }

        int IVsUpdateSolutionEvents.UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand) {
            return ((IVsUpdateSolutionEvents)_inner).UpdateSolution_Done(fSucceeded, fModified, fCancelCommand);
        }

        int IVsUpdateSolutionEvents.UpdateSolution_StartUpdate(ref int pfCancelUpdate) {
            return ((IVsUpdateSolutionEvents)_inner).UpdateSolution_StartUpdate(ref pfCancelUpdate);
        }

        #endregion

        #region INewProjectInitializationProvider Members

        void INewProjectInitializationProvider.InitializeNewProject() {
            ((INewProjectInitializationProvider)_inner).InitializeNewProject();
        }

        #endregion

        #region IVsBrowseObjectContext Members

        VisualStudio.Project.ConfiguredProject IVsBrowseObjectContext.ConfiguredProject {
            get {
                return ((IVsBrowseObjectContext)_inner).ConfiguredProject;
            }
        }

        VisualStudio.Project.Properties.IProjectPropertiesContext IVsBrowseObjectContext.ProjectPropertiesContext {
            get { return ((IVsBrowseObjectContext)_inner).ProjectPropertiesContext; }
        }

        VisualStudio.Project.Properties.IPropertySheet IVsBrowseObjectContext.PropertySheet {
            get { return ((IVsBrowseObjectContext)_inner).PropertySheet; }
        }

        VisualStudio.Project.UnconfiguredProject IVsBrowseObjectContext.UnconfiguredProject {
            get { return ((IVsBrowseObjectContext)_inner).UnconfiguredProject; }
        }

        #endregion

        #region IVsProjectFlavorReferences Members

        int IVsProjectFlavorReferences.QueryAddProjectReference(object pReferencedProject, out int pbCanAdd) {
            return ((IVsProjectFlavorReferences)_inner).QueryAddProjectReference(pReferencedProject, out pbCanAdd);
        }

        int IVsProjectFlavorReferences.QueryCanBeReferenced(object pReferencingProject, out int pbAllowReferenced) {
            return ((IVsProjectFlavorReferences)_inner).QueryCanBeReferenced(pReferencingProject, out pbAllowReferenced);
        }

        int IVsProjectFlavorReferences.QueryRefreshReferences(uint Reason, out int pbUpdate) {
            return ((IVsProjectFlavorReferences)_inner).QueryRefreshReferences(Reason, out pbUpdate);
        }

        #endregion

        #region IHostObjectProvider Members

        VisualStudio.Project.Immutables.IImmutableSet<IHostObject> IHostObjectProvider.HostObjects {
            get {
                return ((IHostObjectProvider)_inner).HostObjects;
            }
        }

        #endregion

        #region IVsFileBackup Members

        int IVsFileBackup.BackupFile(string pszBackupFileName) {
            return ((IVsFileBackup)_inner).BackupFile(pszBackupFileName);
        }

        int IVsFileBackup.IsBackupFileObsolete(out int pbObsolete) {
            return ((IVsFileBackup)_inner).IsBackupFileObsolete(out pbObsolete);
        }

        #endregion

        #region IVsProjectSpecificEditorMap Members

        int IVsProjectSpecificEditorMap.GetSpecificEditorType(string pszMkDocument, out Guid pguidEditorType) {
            return ((IVsProjectSpecificEditorMap)_inner).GetSpecificEditorType(pszMkDocument, out pguidEditorType);
        }

        #endregion

        #region IVsProjectFaultResolver Members

        void IVsProjectFaultResolver.ResolveFault(out bool pfShouldReload) {
            ((IVsProjectFaultResolver)_inner).ResolveFault(out pfShouldReload);
        }

        #endregion

        #region IVsManifestReferenceResolver Members

        IVsTask IVsManifestReferenceResolver.ResolveReferenceAsync(string reference, string relativeToFile) {
            return ((IVsManifestReferenceResolver)_inner).ResolveReferenceAsync(reference, relativeToFile);
        }

        #endregion

        #region IVsUIHierarchy Members

        int IVsUIHierarchy.AdviseHierarchyEvents(IVsHierarchyEvents pEventSink, out uint pdwCookie) {
            return ((IVsUIHierarchy)_inner).AdviseHierarchyEvents(pEventSink, out pdwCookie);
        }

        int IVsUIHierarchy.Close() {
            return ((IVsUIHierarchy)_inner).Close();
        }

        int IVsUIHierarchy.ExecCommand(uint itemid, ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            return ((IVsUIHierarchy)_inner).ExecCommand(itemid, ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        int IVsUIHierarchy.GetCanonicalName(uint itemid, out string pbstrName) {
            return ((IVsUIHierarchy)_inner).GetCanonicalName(itemid, out pbstrName);
        }

        int IVsUIHierarchy.GetGuidProperty(uint itemid, int propid, out Guid pguid) {
            return ((IVsUIHierarchy)_inner).GetGuidProperty(itemid, propid, out pguid);
        }

        int IVsUIHierarchy.GetNestedHierarchy(uint itemid, ref Guid iidHierarchyNested, out IntPtr ppHierarchyNested, out uint pitemidNested) {
            return ((IVsUIHierarchy)_inner).GetNestedHierarchy(itemid, ref iidHierarchyNested, out ppHierarchyNested, out pitemidNested);
        }

        int IVsUIHierarchy.GetProperty(uint itemid, int propid, out object pvar) {
            return ((IVsUIHierarchy)_inner).GetProperty(itemid, propid, out pvar);
        }

        int IVsUIHierarchy.GetSite(out VisualStudio.OLE.Interop.IServiceProvider ppSP) {
            return ((IVsUIHierarchy)_inner).GetSite(out ppSP);
        }

        int IVsUIHierarchy.ParseCanonicalName(string pszName, out uint pitemid) {
            return ((IVsUIHierarchy)_inner).ParseCanonicalName(pszName, out pitemid);
        }

        int IVsUIHierarchy.QueryClose(out int pfCanClose) {
            return ((IVsUIHierarchy)_inner).QueryClose(out pfCanClose);
        }

        int IVsUIHierarchy.QueryStatusCommand(uint itemid, ref Guid pguidCmdGroup, uint cCmds, VisualStudio.OLE.Interop.OLECMD[] prgCmds, IntPtr pCmdText) {
            return ((IVsUIHierarchy)_inner).QueryStatusCommand(itemid, ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        int IVsUIHierarchy.SetGuidProperty(uint itemid, int propid, ref Guid rguid) {
            return ((IVsUIHierarchy)_inner).SetGuidProperty(itemid, propid, ref rguid);
        }

        int IVsUIHierarchy.SetProperty(uint itemid, int propid, object var) {
            return ((IVsUIHierarchy)_inner).SetProperty(itemid, propid, var);
        }

        int IVsUIHierarchy.SetSite(VisualStudio.OLE.Interop.IServiceProvider psp) {
            return ((IVsUIHierarchy)_inner).SetSite(psp);
        }

        int IVsUIHierarchy.UnadviseHierarchyEvents(uint dwCookie) {
            return ((IVsUIHierarchy)_inner).UnadviseHierarchyEvents(dwCookie);
        }

        int IVsUIHierarchy.Unused0() {
            return ((IVsUIHierarchy)_inner).Unused0();
        }

        int IVsUIHierarchy.Unused1() {
            return ((IVsUIHierarchy)_inner).Unused1();
        }

        int IVsUIHierarchy.Unused2() {
            return ((IVsUIHierarchy)_inner).Unused2();
        }

        int IVsUIHierarchy.Unused3() {
            return ((IVsUIHierarchy)_inner).Unused3();
        }

        int IVsUIHierarchy.Unused4() {
            return ((IVsUIHierarchy)_inner).Unused4();
        }

        #endregion

        #region IVsHierarchy Members

        int IVsHierarchy.AdviseHierarchyEvents(IVsHierarchyEvents pEventSink, out uint pdwCookie) {
            return ((IVsHierarchy)_inner).AdviseHierarchyEvents(pEventSink, out pdwCookie);
        }

        int IVsHierarchy.Close() {
            return ((IVsHierarchy)_inner).Close();
        }

        int IVsHierarchy.GetCanonicalName(uint itemid, out string pbstrName) {
            return ((IVsHierarchy)_inner).GetCanonicalName(itemid, out pbstrName);
        }

        int IVsHierarchy.GetGuidProperty(uint itemid, int propid, out Guid pguid) {
            return ((IVsHierarchy)_inner).GetGuidProperty(itemid, propid, out pguid);
        }

        int IVsHierarchy.GetNestedHierarchy(uint itemid, ref Guid iidHierarchyNested, out IntPtr ppHierarchyNested, out uint pitemidNested) {
            return ((IVsHierarchy)_inner).GetNestedHierarchy(itemid, ref iidHierarchyNested, out ppHierarchyNested, out pitemidNested);
        }

        int IVsHierarchy.GetProperty(uint itemid, int propid, out object pvar) {
            if (propid == (int)__VSHPROPID.VSHPROPID_ExtObject) {
                int res = ((IVsHierarchy)_inner).GetProperty(itemid, propid, out pvar);
                if (ErrorHandler.Succeeded(res) && pvar is EnvDTE.Project) {
                    pvar = new OAProject((EnvDTE.Project)pvar, this);
                }
                return res;
            }

            switch ((__VSHPROPID4)propid) {

                case __VSHPROPID4.VSHPROPID_TargetFrameworkMoniker:
                    // really only here for testing so WAP projects load correctly...
                    // But this also impacts the toolbox by filtering what available items there are.
                    pvar = ".NETFramework,Version=v4.0,Profile=Client";
                    return VSConstants.S_OK;
            }

            return ((IVsHierarchy)_inner).GetProperty(itemid, propid, out pvar);
        }

        int IVsHierarchy.GetSite(out VisualStudio.OLE.Interop.IServiceProvider ppSP) {
            return ((IVsHierarchy)_inner).GetSite(out ppSP);
        }

        int IVsHierarchy.ParseCanonicalName(string pszName, out uint pitemid) {
            return ((IVsHierarchy)_inner).ParseCanonicalName(pszName, out pitemid);
        }

        int IVsHierarchy.QueryClose(out int pfCanClose) {
            return ((IVsHierarchy)_inner).QueryClose(out pfCanClose);
        }

        int IVsHierarchy.SetGuidProperty(uint itemid, int propid, ref Guid rguid) {
            return ((IVsHierarchy)_inner).SetGuidProperty(itemid, propid, ref rguid);
        }

        int IVsHierarchy.SetProperty(uint itemid, int propid, object var) {
            return ((IVsHierarchy)_inner).SetProperty(itemid, propid, var);
        }

        int IVsHierarchy.SetSite(VisualStudio.OLE.Interop.IServiceProvider psp) {
            return ((IVsHierarchy)_inner).SetSite(psp);
        }

        int IVsHierarchy.UnadviseHierarchyEvents(uint dwCookie) {
            return ((IVsHierarchy)_inner).UnadviseHierarchyEvents(dwCookie);
        }

        int IVsHierarchy.Unused0() {
            return ((IVsHierarchy)_inner).Unused0();
        }

        int IVsHierarchy.Unused1() {
            return ((IVsHierarchy)_inner).Unused1();
        }

        int IVsHierarchy.Unused2() {
            return ((IVsHierarchy)_inner).Unused2();
        }

        int IVsHierarchy.Unused3() {
            return ((IVsHierarchy)_inner).Unused3();
        }

        int IVsHierarchy.Unused4() {
            return ((IVsHierarchy)_inner).Unused4();
        }

        #endregion

        #region IVsPersistHierarchyItem2 Members

        int IVsPersistHierarchyItem2.IgnoreItemFileChanges(uint itemid, int fIgnore) {
            return ((IVsPersistHierarchyItem2)_inner).IgnoreItemFileChanges(itemid, fIgnore);
        }

        int IVsPersistHierarchyItem2.IsItemDirty(uint itemid, IntPtr punkDocData, out int pfDirty) {
            return ((IVsPersistHierarchyItem2)_inner).IsItemDirty(itemid, punkDocData, out pfDirty);
        }

        int IVsPersistHierarchyItem2.IsItemReloadable(uint itemid, out int pfReloadable) {
            return ((IVsPersistHierarchyItem2)_inner).IsItemReloadable(itemid, out pfReloadable);
        }

        int IVsPersistHierarchyItem2.ReloadItem(uint itemid, uint dwReserved) {
            return ((IVsPersistHierarchyItem2)_inner).ReloadItem(itemid, dwReserved);
        }

        int IVsPersistHierarchyItem2.SaveItem(VSSAVEFLAGS dwSave, string pszSilentSaveAsName, uint itemid, IntPtr punkDocData, out int pfCanceled) {
            return ((IVsPersistHierarchyItem2)_inner).SaveItem(dwSave, pszSilentSaveAsName, itemid, punkDocData, out pfCanceled);
        }

        #endregion

        #region IVsPersistHierarchyItem Members

        int IVsPersistHierarchyItem.IsItemDirty(uint itemid, IntPtr punkDocData, out int pfDirty) {
            return ((IVsPersistHierarchyItem)_inner).IsItemDirty(itemid, punkDocData, out pfDirty);
        }

        int IVsPersistHierarchyItem.SaveItem(VSSAVEFLAGS dwSave, string pszSilentSaveAsName, uint itemid, IntPtr punkDocData, out int pfCanceled) {
            return ((IVsPersistHierarchyItem)_inner).SaveItem(dwSave, pszSilentSaveAsName, itemid, punkDocData, out pfCanceled);
        }

        #endregion

        #region IOleCommandTarget Members

        int VisualStudio.OLE.Interop.IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut) {
            return ((VisualStudio.OLE.Interop.IOleCommandTarget)_inner).Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        int VisualStudio.OLE.Interop.IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, VisualStudio.OLE.Interop.OLECMD[] prgCmds, IntPtr pCmdText) {
            if (_inner != null) {
                return ((VisualStudio.OLE.Interop.IOleCommandTarget)_inner).QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
            }
            return VSConstants.E_FAIL;
        }

        #endregion

        #region IVsHierarchyDeleteHandler Members

        int IVsHierarchyDeleteHandler.DeleteItem(uint dwDelItemOp, uint itemid) {
            return ((IVsHierarchyDeleteHandler)_inner).DeleteItem(dwDelItemOp, itemid);
        }

        int IVsHierarchyDeleteHandler.QueryDeleteItem(uint dwDelItemOp, uint itemid, out int pfCanDelete) {
            return ((IVsHierarchyDeleteHandler)_inner).QueryDeleteItem(dwDelItemOp, itemid, out pfCanDelete);
        }

        #endregion

        #region IVsHierarchyDeleteHandler3 Members

        int IVsHierarchyDeleteHandler3.DeleteItems(uint cItems, uint dwDelItemOp, uint[] itemid, uint dwFlags) {
            return ((IVsHierarchyDeleteHandler3)_inner).DeleteItems(cItems, dwDelItemOp, itemid, dwFlags);
        }

        int IVsHierarchyDeleteHandler3.QueryDeleteItems(uint cItems, uint dwDelItemOp, uint[] itemid, bool[] pfCanDelete) {
            return ((IVsHierarchyDeleteHandler3)_inner).QueryDeleteItems(cItems, dwDelItemOp, itemid, pfCanDelete);
        }

        #endregion

        #region IVsAggregatableProject Members

        public int SetInnerProject(object punkInner) {
            return VSConstants.S_OK;
        }

        #endregion
    }
#endif
}
