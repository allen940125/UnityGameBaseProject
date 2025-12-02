using UnityEngine;

public class AutoInstancingSetup : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("作為 Instancing 原型的 GameObject。必須包含 Mesh Renderer 且材質已勾選 GPU Instancing。")]
    public GameObject prototype;

    [Tooltip("每軸物件數量 (例如 32 -> 32x32=1024 個物件)")]
    public int countPerAxis = 32;

    [Tooltip("物件之間的間距")]
    public float spacing = 2f;

    void Start()
    {
        // 修正後的錯誤檢查：檢查原型物件（或其子物件）是否包含任何 MeshRenderer
        // GetComponentInChildren(true) 會在整個階層中尋找 Renderer
        if (prototype == null || prototype.GetComponentInChildren<MeshRenderer>(true) == null)
        {
            Debug.LogError("原型物件 (Prototype) 必須設定且在其階層中具有 MeshRenderer 組件！");
            return;
        }

        // 確保原型物件在場景中是隱藏的，避免它被重複繪製
        // 注意：如果您拖入的是 Prefab Asset，這裡的 SetActive(false) 是安全的。
        prototype.SetActive(false);

        int total = countPerAxis * countPerAxis;

        for (int i = 0; i < total; i++)
        {
            int x = i % countPerAxis;
            int y = i / countPerAxis;

            // 1. 計算位置
            Vector3 pos = new Vector3(
                (x - countPerAxis / 2f) * spacing,
                0f,
                (y - countPerAxis / 2f) * spacing);

            // 2. 實例化新的 GameObject
            // 注意：Instantiate 應該複製 Prefab Asset，而非場景中的物件，以避免產生大量實例物件
            GameObject instance = Instantiate(prototype, this.transform);
            instance.transform.position = pos;
            instance.name = $"Instance_{i}";
            instance.SetActive(true);

            // 取得渲染器，這裡會取得 Prototype 及其所有子物件上的所有 MeshRenderer
            MeshRenderer[] renderers = instance.GetComponentsInChildren<MeshRenderer>(true);
            
            // 由於前面已經檢查過至少有一個 Renderer，這裡理論上不會為 0
            
            // 計算顏色
            Color c = Color.HSVToRGB((float)i / total, 0.8f, 1f);

            // 針對每個子 Mesh Renderer 設定 Property Block
            foreach (MeshRenderer renderer in renderers)
            {
                // 透過 MaterialPropertyBlock 設置 Per-Instance 屬性
                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(mpb); 
                
                // 將顏色設定到 MPB 中
                mpb.SetColor("_Color", c); 

                // 應用 MPB
                renderer.SetPropertyBlock(mpb);
            }
        }
        
        Debug.Log($"自動 Instancing 設定完成，共生成 {total} 個獨立物件。請在 Frame Debugger 中檢查 Draw Call 數量。");
    }
}