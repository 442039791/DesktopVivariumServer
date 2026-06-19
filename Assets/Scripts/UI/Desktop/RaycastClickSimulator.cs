using UnityEngine;
using UnityEngine.UI;

public class RaycastClickSimulator : MonoBehaviour
{
    //public LayerMask uiLayerMask;  // 指定 UI 层的 LayerMask

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 检测鼠标左键点击
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity);

            if (hit.collider != null)
            {

                // 获取按钮组件并模拟点击
                Button button = hit.collider.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.Invoke(); // 模拟按钮点击事件
                }
                else
                {
                }
            }
            else
            {
            }
        }
    }
}
