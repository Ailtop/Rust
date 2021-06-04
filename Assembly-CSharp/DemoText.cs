using Rust.UI;
using UnityEngine;

public class DemoText : SingletonComponent<DemoText>
{
	public RustText TimeText;

	public RustText TotalSecondText;

	public RustText TimeScaleText;

	public RustText FilenameText;

	public RustText DateTimeText;

	public RustText ParentText;

	public RustText DofText;

	public GameObject InternalRoot;

	public GameObject ParentRoot;

	public GameObject DofRoot;
}
