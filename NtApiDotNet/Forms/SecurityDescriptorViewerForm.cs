﻿//  Copyright 2018 Google Inc. All Rights Reserved.
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

using NtApiDotNet.Win32;
using System.Windows.Forms;

namespace NtApiDotNet.Forms
{
    /// <summary>
    /// Form to view an object's security descriptor.
    /// </summary>
    public partial class SecurityDescriptorViewerForm : Form
    {
        private NtObject _obj;

        private static SecurityDescriptor GetSecurityDescriptor(NtObject obj)
        {
            SecurityInformation security_info = 0;
            if (obj.IsAccessMaskGranted(GenericAccessRights.ReadControl))
            {
                security_info = SecurityInformation.AllBasic;
            }
            if (obj.IsAccessMaskGranted(GenericAccessRights.AccessSystemSecurity))
            {
                security_info |= SecurityInformation.Sacl;
            }
            if (security_info == 0)
            {
                // We can't seem to tell what access we have, just try and get basic information.
                security_info = SecurityInformation.AllBasic;
            }

            return obj.GetSecurityDescriptor(security_info);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the object.</param>
        /// <param name="security_descriptor">Security descriptor to view.</param>
        public SecurityDescriptorViewerForm(string name, SecurityDescriptor security_descriptor) 
            : this(name, security_descriptor, security_descriptor.NtType, false)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the object.</param>
        /// <param name="security_descriptor">Security descriptor to view.</param>
        /// <param name="type">NT type for view.</param>
        /// <param name="is_container">True to indicate this object is a container.</param>
        public SecurityDescriptorViewerForm(string name, SecurityDescriptor security_descriptor, NtType type, bool is_container)
        {
            InitializeComponent();
            Text = $"Security for {name}";
            securityDescriptorViewerControl.SetSecurityDescriptor(security_descriptor, type, type.ValidAccess, is_container);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="obj">The object to view.</param>
        /// <param name="read_only">True to specify read only viewer.</param>
        public SecurityDescriptorViewerForm(NtObject obj, bool read_only) 
            : this(obj.Name, GetSecurityDescriptor(obj), obj.NtType, obj.IsContainer)
        {
            if (obj.IsAccessMaskGranted(GenericAccessRights.WriteDac) && !read_only)
            {
                btnEditPermissions.Enabled = true;
            }
            _obj = obj;
        }

        private void btnEditPermissions_Click(object sender, System.EventArgs e)
        {
            Win32Utils.EditSecurity(this.Handle, _obj, _obj.Name, false);
        }
    }
}
