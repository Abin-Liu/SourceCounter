using System;
using System.Reflection;
using System.Windows.Forms;

namespace AbinLibs
{
	/// <summary>
	/// Wraps System.Windows.Forms.OpenFileDialog to make it present
	/// a vista-style dialog.
	/// </summary>
	public class OpenFolderDialog
	{
		// Wrapped dialog
		private OpenFileDialog m_ofd = null;

		/// <summary>
		/// Default constructor
		/// </summary>
		public OpenFolderDialog()
		{
			m_ofd = new OpenFileDialog();
			m_ofd.Filter = "Folders|\n";
			m_ofd.AddExtension = false;
			m_ofd.CheckFileExists = false;
			m_ofd.DereferenceLinks = true;
			m_ofd.Multiselect = false;
		}

		#region Properties		

		/// <summary>
		/// Gets/Sets the title to show in the dialog
		/// </summary>
		public string Title
		{
			get { return m_ofd.Title; }
			set { m_ofd.Title = value == null ? "Select a folder" : value; }
		}

		/// <summary>
		/// Gets/Sets the selected folder
		/// </summary>
		public string SelectedPath
		{
			get
			{
				return m_ofd.FileName;
			}

			set
			{
				if (string.IsNullOrEmpty(value))
				{
					m_ofd.InitialDirectory = Environment.CurrentDirectory;
				}
				else
				{
					m_ofd.InitialDirectory = value;
				}
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Shows the dialog
		/// </summary>
		/// <returns>True if the user presses OK else false</returns>
		public DialogResult ShowDialog()
		{
			return ShowDialog(null);
		}

		/// <summary>
		/// Shows the dialog
		/// </summary>
		/// <param name="owner">Parent window</param>
		/// <returns>Result of the dialog</returns>
		public DialogResult ShowDialog(IWin32Window owner)
		{
			DialogResult result = DialogResult.Cancel;

			if (Environment.OSVersion.Version.Major >= 6)
			{
				var r = new Reflector("System.Windows.Forms");

				uint num = 0;
				Type typeIFileDialog = r.GetType("FileDialogNative.IFileDialog");
				object dialog = r.Call(m_ofd, "CreateVistaDialog");
				r.Call(m_ofd, "OnBeforeVistaDialog", dialog);

				uint options = (uint)r.CallAs(typeof(FileDialog), m_ofd, "GetOptions");
				options |= (uint)r.GetEnum("FileDialogNative.FOS", "FOS_PICKFOLDERS");
				r.CallAs(typeIFileDialog, dialog, "SetOptions", options);

				object pfde = r.New("FileDialog.VistaDialogEvents", m_ofd);
				object[] parameters = new object[] { pfde, num };
				r.CallAs2(typeIFileDialog, dialog, "Advise", parameters);
				num = (uint)parameters[1];
				try
				{
					int num2 = (int)r.CallAs(typeIFileDialog, dialog, "Show", owner == null ? IntPtr.Zero : owner.Handle);
					if (num2 == 0)
					{
						result = DialogResult.OK;
					}					
				}
				finally
				{
					r.CallAs(typeIFileDialog, dialog, "Unadvise", num);
					GC.KeepAlive(pfde);
				}
			}
			else
			{
				var fbd = new FolderBrowserDialog();
				fbd.Description = this.Title;
				fbd.SelectedPath = this.SelectedPath;
				fbd.ShowNewFolderButton = false;
				result = fbd.ShowDialog(owner);
				if (result == DialogResult.OK)
				{
					m_ofd.FileName = fbd.SelectedPath;
				}				
			}

			return result;
		}

		#endregion
	}

	/// <summary>
	/// Creates IWin32Window around an IntPtr
	/// </summary>
	public class WindowWrapper : IWin32Window
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="handle">Handle to wrap</param>
		public WindowWrapper(IntPtr handle)
		{
			Handle = handle;
		}

		/// <summary>
		/// Original ptr
		/// </summary>
		public IntPtr Handle { get; private set; }
	}

	/// <summary>
	/// This class is from the Front-End for Dosbox and is used to present a 'vista' dialog box to select folders.
	/// Being able to use a vista style dialog box to select folders is much better then using the shell folder browser.
	/// http://code.google.com/p/fed/
	///
	/// Example:
	/// var r = new Reflector("System.Windows.Forms");
	/// </summary>
	public class Reflector
	{
		#region variables

		string m_ns;
		Assembly m_asmb;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="ns">The namespace containing types to be used</param>
		public Reflector(string ns)
			: this(ns, ns)
		{ }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="an">A specific assembly name (used if the assembly name does not tie exactly with the namespace)</param>
		/// <param name="ns">The namespace containing types to be used</param>
		public Reflector(string an, string ns)
		{
			m_ns = ns;
			m_asmb = null;
			foreach (AssemblyName aN in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
			{
				if (aN.FullName.StartsWith(an))
				{
					m_asmb = Assembly.Load(aN);
					break;
				}
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Return a Type instance for a type 'typeName'
		/// </summary>
		/// <param name="typeName">The name of the type</param>
		/// <returns>A type instance</returns>
		public Type GetType(string typeName)
		{
			Type type = null;
			string[] names = typeName.Split('.');

			if (names.Length > 0)
				type = m_asmb.GetType(m_ns + "." + names[0]);

			for (int i = 1; i < names.Length; ++i)
			{
				type = type.GetNestedType(names[i], BindingFlags.NonPublic);
			}
			return type;
		}

		/// <summary>
		/// Create a new object of a named type passing along any params
		/// </summary>
		/// <param name="name">The name of the type to create</param>
		/// <param name="parameters"></param>
		/// <returns>An instantiated type</returns>
		public object New(string name, params object[] parameters)
		{
			Type type = GetType(name);

			ConstructorInfo[] ctorInfos = type.GetConstructors();
			foreach (ConstructorInfo ci in ctorInfos)
			{
				try
				{
					return ci.Invoke(parameters);
				}
				catch { }
			}

			return null;
		}

		/// <summary>
		/// Calls method 'func' on object 'obj' passing parameters 'parameters'
		/// </summary>
		/// <param name="obj">The object on which to excute function 'func'</param>
		/// <param name="func">The function to execute</param>
		/// <param name="parameters">The parameters to pass to function 'func'</param>
		/// <returns>The result of the function invocation</returns>
		public object Call(object obj, string func, params object[] parameters)
		{
			return Call2(obj, func, parameters);
		}

		/// <summary>
		/// Calls method 'func' on object 'obj' passing parameters 'parameters'
		/// </summary>
		/// <param name="obj">The object on which to excute function 'func'</param>
		/// <param name="func">The function to execute</param>
		/// <param name="parameters">The parameters to pass to function 'func'</param>
		/// <returns>The result of the function invocation</returns>
		public object Call2(object obj, string func, object[] parameters)
		{
			return CallAs2(obj.GetType(), obj, func, parameters);
		}

		/// <summary>
		/// Calls method 'func' on object 'obj' which is of type 'type' passing parameters 'parameters'
		/// </summary>
		/// <param name="type">The type of 'obj'</param>
		/// <param name="obj">The object on which to excute function 'func'</param>
		/// <param name="func">The function to execute</param>
		/// <param name="parameters">The parameters to pass to function 'func'</param>
		/// <returns>The result of the function invocation</returns>
		public object CallAs(Type type, object obj, string func, params object[] parameters)
		{
			return CallAs2(type, obj, func, parameters);
		}

		/// <summary>
		/// Calls method 'func' on object 'obj' which is of type 'type' passing parameters 'parameters'
		/// </summary>
		/// <param name="type">The type of 'obj'</param>
		/// <param name="obj">The object on which to excute function 'func'</param>
		/// <param name="func">The function to execute</param>
		/// <param name="parameters">The parameters to pass to function 'func'</param>
		/// <returns>The result of the function invocation</returns>
		public object CallAs2(Type type, object obj, string func, object[] parameters)
		{
			MethodInfo methInfo = type.GetMethod(func, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			return methInfo.Invoke(obj, parameters);
		}

		/// <summary>
		/// Returns the value of property 'prop' of object 'obj'
		/// </summary>
		/// <param name="obj">The object containing 'prop'</param>
		/// <param name="prop">The property name</param>
		/// <returns>The property value</returns>
		public object Get(object obj, string prop)
		{
			return GetAs(obj.GetType(), obj, prop);
		}

		/// <summary>
		/// Returns the value of property 'prop' of object 'obj' which has type 'type'
		/// </summary>
		/// <param name="type">The type of 'obj'</param>
		/// <param name="obj">The object containing 'prop'</param>
		/// <param name="prop">The property name</param>
		/// <returns>The property value</returns>
		public object GetAs(Type type, object obj, string prop)
		{
			PropertyInfo propInfo = type.GetProperty(prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			return propInfo.GetValue(obj, null);
		}

		/// <summary>
		/// Returns an enum value
		/// </summary>
		/// <param name="typeName">The name of enum type</param>
		/// <param name="name">The name of the value</param>
		/// <returns>The enum value</returns>
		public object GetEnum(string typeName, string name)
		{
			Type type = GetType(typeName);
			FieldInfo fieldInfo = type.GetField(name);
			return fieldInfo.GetValue(null);
		}
		#endregion

	}
}
