﻿//  Copyright 2016, 2017 Google Inc. All Rights Reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using NtApiDotNet;
using System.Management.Automation;

namespace NtObjectManager.Cmdlets.Object
{
    /// <summary>
    /// <para type="synopsis">Open a NT job object by path.</para>
    /// <para type="description">This cmdlet opens an existing NT job object. The absolute path to the object in the NT object manager name space must be specified. 
    /// It's also possible to create the object relative to an existing object by specified the -Root parameter.</para>
    /// </summary>
    /// <example>
    ///   <code>$obj = Get-NtJob \BaseNamedObjects\ABC</code>
    ///   <para>Get an job object with an absolute path.</para>
    /// </example>
    /// <example>
    ///   <code>$root = Get-NtDirectory \BaseNamedObjects&#x0A;$obj = Get-NtJob ABC -Root $root</code>
    ///   <para>Get an job object with a relative path.</para>
    /// </example>
    /// <example>
    ///   <code>cd NtObject:\BaseNamedObjects&#x0A;$obj = Get-NtJob ABC</code>
    ///   <para>Get a job object with a relative path based on the current location.
    ///   </para>
    /// </example>
    /// <para type="link">about_ManagingNtObjectLifetime</para>
    [Cmdlet(VerbsCommon.Get, "NtJob")]
    [OutputType(typeof(NtJob))]
    public sealed class GetNtJobCmdlet : NtObjectBaseCmdletWithAccess<JobAccessRights>
    {
        /// <summary>
        /// Determine if the cmdlet can create objects.
        /// </summary>
        /// <returns>True if objects can be created.</returns>
        protected override bool CanCreateDirectories()
        {
            return false;
        }

        /// <summary>
        /// <para type="description">The NT object manager path to the object to use.</para>
        /// </summary>
        [Parameter(Position = 0, Mandatory = true)]
        public override string Path { get; set; }

        /// <summary>
        /// Method to create an object from a set of object attributes.
        /// </summary>
        /// <param name="obj_attributes">The object attributes to create/open from.</param>
        /// <returns>The newly created object.</returns>
        protected override object CreateObject(ObjectAttributes obj_attributes)
        {
            return NtJob.Open(obj_attributes, Access);
        }
    }

    /// <summary>
    /// <para type="synopsis">Create a new NT job object.</para>
    /// <para type="description">This cmdlet creates a new NT job object. The absolute path to the object in the NT object manager name space can be specified. 
    /// It's also possible to create the object relative to an existing object by specified the -Root parameter. If no path is specified than an unnamed object will be created which
    /// can only be duplicated by handle.</para>
    /// </summary>
    /// <example>
    ///   <code>$obj = New-NtJob</code>
    ///   <para>Create a new anonymous job object.</para>
    /// </example>
    /// <example>
    ///   <code>$obj = New-NtJob \BaseNamedObjects\ABC</code>
    ///   <para>Create a new job object with an absolute path.</para>
    /// </example>
    /// <example>
    ///   <code>$root = Get-NtDirectory \BaseNamedObjects&#x0A;$obj = New-NtJob ABC -Root $root</code>
    ///   <para>Create a new job object with a relative path.
    ///   </para>
    /// </example>
    /// <example>
    ///   <code>cd NtObject:\BaseNamedObjects&#x0A;$obj = New-NtJob ABC</code>
    ///   <para>Create a new job object with a relative path based on the current location.
    ///   </para>
    /// </example>
    /// <para type="link">about_ManagingNtObjectLifetime</para>
    [Cmdlet(VerbsCommon.New, "NtJob")]
    [OutputType(typeof(NtJob))]
    public sealed class NewNtJobCmdlet : NtObjectBaseCmdletWithAccess<JobAccessRights>
    {
        /// <summary>
        /// <para type="description">Specify a process limit for the job.</para>
        /// </summary>
        [Parameter]
        public int ActiveProcessLimit { get; set; }

        /// <summary>
        /// <para type="description">Specify limit flags for the job.</para>
        /// </summary>
        [Parameter]
        public JobObjectLimitFlags LimitFlags { get; set; }

        /// <summary>
        /// <para type="description">Specify UI Restriction flags for the job.</para>
        /// </summary>
        [Parameter]
        public JobObjectUiLimitFlags UiRestrictionFlags { get; set; }

        /// <summary>
        /// Determine if the cmdlet can create objects.
        /// </summary>
        /// <returns>True if objects can be created.</returns>
        protected override bool CanCreateDirectories()
        {
            return true;
        }

        /// <summary>
        /// Method to create an object from a set of object attributes.
        /// </summary>
        /// <param name="obj_attributes">The object attributes to create/open from.</param>
        /// <returns>The newly created object.</returns>
        protected override object CreateObject(ObjectAttributes obj_attributes)
        {
            using (var job = NtJob.Create(obj_attributes, Access))
            {
                if (LimitFlags != 0)
                {
                    job.LimitFlags = LimitFlags;
                }
                if (ActiveProcessLimit > 0)
                {
                    job.ActiveProcessLimit = ActiveProcessLimit;
                }
                if (UiRestrictionFlags != 0)
                {
                    job.UiRestrictionFlags = UiRestrictionFlags;
                }
                return job.Duplicate();
            }
        }
    }

    /// <summary>
    /// <para type="synopsis">Assign a process to a Job object.</para>
    /// <para type="description">This cmdlet assigns a process to a Job object.</para>
    /// </summary>
    /// <example>
    ///   <code>Set-NtProcessJob -Job $job -Process $process</code>
    ///   <para>Assigns the process to the job object.</para>
    /// </example>
    /// <para type="link">about_ManagingNtObjectLifetime</para>
    [Cmdlet(VerbsCommon.Set, "NtProcessJob")]
    public sealed class SetNtProcessJobCmdlet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Specify the job object.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public NtJob Job { get; set; }

        /// <summary>
        /// <para type="description">Specify the list of processes to assign.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipeline = true)]
        public NtProcess[] Process { get; set; }

        /// <summary>
        /// <para type="description">Specify to pass through the process objects.</para>
        /// </summary>
        [Parameter]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Overridden ProcessRecord method.
        /// </summary>
        protected override void ProcessRecord()
        {
            foreach (var proc in Process)
            {
                try
                {
                    Job.AssignProcess(proc);
                }
                catch (NtException ex)
                {
                    WriteError(new ErrorRecord(ex, "AssignJob", ErrorCategory.QuotaExceeded, proc));
                }

                if (PassThru)
                {
                    WriteObject(proc);
                }
            }
        }
    }

    /// <summary>
    /// <para type="synopsis">Gets the accessible Job objects assigned to a process.</para>
    /// <para type="description">This cmdlet gets the accessible Job objects for a process. This might not include all Jobs and might contain duplicates.</para>
    /// </summary>
    /// <example>
    ///   <code>Get-NtProcessJob -Process $process</code>
    ///   <para>Gets the Job objects assigned to the process.</para>
    /// </example>
    /// <para type="link">about_ManagingNtObjectLifetime</para>
    [Cmdlet(VerbsCommon.Get, "NtProcessJob")]
    [OutputType(typeof(NtJob))]
    public sealed class GetNtProcessJobCmdlet : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Specify the list of processes to assign.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public NtProcess Process { get; set; }

        /// <summary>
        /// Overridden ProcessRecord method.
        /// </summary>
        protected override void ProcessRecord()
        {
            WriteObject(Process.GetAccessibleJobObjects(), true);
        }
    }
}
