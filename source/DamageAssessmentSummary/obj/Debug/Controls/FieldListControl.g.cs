﻿#pragma checksum "..\..\..\Controls\FieldListControl.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "43D0D04392AD1E3474518CF17FFD1F82"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using ConfigureSummaryReport;
using ConfigureSummaryReport.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace ConfigureSummaryReport.Controls {
    
    
    /// <summary>
    /// FieldListControl
    /// </summary>
    public partial class FieldListControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 44 "..\..\..\Controls\FieldListControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkBoxSelectAll;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\..\Controls\FieldListControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkBoxUseAliasName;
        
        #line default
        #line hidden
        
        
        #line 54 "..\..\..\Controls\FieldListControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox chkBoxUseExpandableList;
        
        #line default
        #line hidden
        
        
        #line 62 "..\..\..\Controls\FieldListControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal ConfigureSummaryReport.Controls.DraggableListView chkBoxListView;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/SummaryReport;component/controls/fieldlistcontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Controls\FieldListControl.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal System.Delegate _CreateDelegate(System.Type delegateType, string handler) {
            return System.Delegate.CreateDelegate(delegateType, this, handler);
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.chkBoxSelectAll = ((System.Windows.Controls.CheckBox)(target));
            
            #line 46 "..\..\..\Controls\FieldListControl.xaml"
            this.chkBoxSelectAll.Click += new System.Windows.RoutedEventHandler(this.chkBoxSelectAll_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.chkBoxUseAliasName = ((System.Windows.Controls.CheckBox)(target));
            
            #line 51 "..\..\..\Controls\FieldListControl.xaml"
            this.chkBoxUseAliasName.Click += new System.Windows.RoutedEventHandler(this.chkBoxSelectAllAliasNames_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.chkBoxUseExpandableList = ((System.Windows.Controls.CheckBox)(target));
            
            #line 56 "..\..\..\Controls\FieldListControl.xaml"
            this.chkBoxUseExpandableList.Click += new System.Windows.RoutedEventHandler(this.chkBoxUseExpandableList_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.chkBoxListView = ((ConfigureSummaryReport.Controls.DraggableListView)(target));
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 5:
            
            #line 78 "..\..\..\Controls\FieldListControl.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Click += new System.Windows.RoutedEventHandler(this.chkBox_Click);
            
            #line default
            #line hidden
            
            #line 79 "..\..\..\Controls\FieldListControl.xaml"
            ((System.Windows.Controls.CheckBox)(target)).Checked += new System.Windows.RoutedEventHandler(this.chkBox_Checked);
            
            #line default
            #line hidden
            break;
            }
        }
    }
}

