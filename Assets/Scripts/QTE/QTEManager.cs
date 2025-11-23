using UnityEngine;
using System.Collections;

public class QTEManager : MonoBehaviour
{
    [SerializeField] private QTEUIController uiController;
    private QTEResult lastQTEResult;

    public IEnumerator ExecuteQTE(QTEPattern pattern)
    {
        BaseQTE qteExecutor = GetQTEExecutor(pattern.qteType);
        QTEResult result = new QTEResult();

        yield return qteExecutor.Execute(pattern, result, uiController);

        lastQTEResult = result;
    }

    public QTEResult GetLastQTEResult()
    {
        return lastQTEResult;
    }

    private BaseQTE GetQTEExecutor(QTEType type)
    {
        switch (type)
        {
            case QTEType.Sequence:
                return gameObject.AddComponent<SequenceQTE>();
            case QTEType.ButtonMash:
                return gameObject.AddComponent<ButtonMashQTE>();
            // Add other types...
            default:
                return gameObject.AddComponent<SequenceQTE>();
        }
    }
}