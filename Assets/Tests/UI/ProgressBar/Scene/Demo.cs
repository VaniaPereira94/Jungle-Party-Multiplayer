using UnityEngine;

public class Demo : MonoBehaviour
{
    public ProgressBar Pb;
    public ProgressBarCircleController PbC;

    private void Start()
    {
        Pb.BarValue = 50;
        PbC.BarValue = 50;
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.G))
        {
            Pb.BarValue += 1;
            PbC.BarValue += 1;
        }

        if (Input.GetKey(KeyCode.F))
        {
            Pb.BarValue -= 1;
            PbC.BarValue -= 1;
        }
    }
}