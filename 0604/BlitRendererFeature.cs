using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

// ?SRF��� 3 ����v pass�F
// BlitStartRenderPass�F��?�����I?�F?�t��?�����o���i�y? Blit ?��?���j
// BlitRenderPass�F��?��?������??�s�S�� Blit ����
// BlitEndRenderPass�F����??���@�I?���� Blit ��?����
public class BlitRendererFeature : ScriptableRendererFeature
{
    // �꘢��?�� ContextContainer ���I����?�㉺��?�C??�Ǘ�?���iUnity �I per-frame �����e��j
    public class BlitData : ContextItem, IDisposable
    {
        // �ݑ��� Blit ���쒆?����??��?�o�C��ƒ�??�ʔ핢᳁B�iPing-Pong �o?�t�j
        // RTHandle �� Unity �I RenderTexture ����?�i�ו�����?�z�j
        RTHandle m_TextureFront;
        RTHandle m_TextureBack;
        // TextureHandle �� RenderGraph �p���Ǘ�?����?�I�啿�i?????�j
        TextureHandle m_TextureHandleFront;
        TextureHandle m_TextureHandleBack;

        // scaleBias ������?�z�p�I
        static Vector4 scaleBias = new Vector4(1f, 1f, 0f, 0f);

        // �T�����O�����I��?��??���iping-pong�j
        bool m_IsFront = true;

        // �\�����O�g�L��?�ʁh�I?���啿
        public TextureHandle texture;

        // �g�p?�������O�I RenderTextureDescriptor ���n�� RTHandle
        // �R�@��? renderGraph.ImportTexture ?�� TextureHandle�C�ȋ� RenderGraph �g�p
        // ??��?�p?��?�I class �V�O��?�p
        public void Init(RenderGraph renderGraph, RenderTextureDescriptor targetDescriptor, string textureName = null)
        {
            var texName = String.IsNullOrEmpty(textureName) ? "_BlitTexture" : textureName;
            // �?�d���z�ڐ�?���I RTHandle�CReAllocateHandleIfNeeded�?�c texName ���e?�S��?��
            RenderingUtils.ReAllocateHandleIfNeeded(ref m_TextureFront, targetDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: texName + "Front");
            RenderingUtils.ReAllocateHandleIfNeeded(ref m_TextureBack, targetDescriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: texName + "Back");

            m_TextureHandleFront = renderGraph.ImportTexture(m_TextureFront);
            m_TextureHandleBack = renderGraph.ImportTexture(m_TextureBack);
            // ?�u���n�L��?��? m_TextureHandleFront
            texture = m_TextureHandleFront;
        }

        // RenderGraph ���ۑ����O??���C���Ȏ��v??�d�u
        public override void Reset()
        {
            // ���� TextureHandle
            m_TextureHandleFront = TextureHandle.nullHandle;
            m_TextureHandleBack = TextureHandle.nullHandle;
            texture = TextureHandle.nullHandle;
            // ��? m_IsFront ? true�C�d�V�� Front ?�n
            m_IsFront = true;
        }

        // �p��?�꘢ Blit Pass �I?��?�o??��
        // ?�꘢ blit pass �s���v�m���F����?���i�ʏ퐥��ꎟ�I?�ʁj�C?�o?���i�o?�t���I?�꘢�j�C?���g�p�I��?�i��?���\��?������ shader�j
        class PassData
        {
            public TextureHandle source;
            public TextureHandle destination;
            public Material material;
        }

        // ?����?�� �� texture
        public void RecordBlitColor(RenderGraph renderGraph, ContextContainer frameData)
        {
            // �g�p !IsValid() �����f �g?��?�I�啿���ۛ�??�u?�h�B�v?�u?�F���n��?���a�啿�B?�u?�F?��?��?��?�y?�D?��?���C�Ȓ��ڎg�p�B
            if (!texture.IsValid())
            {
                // �\�����O?�����I��??���i���i�ڐ��A?�F�i�����j
                var cameraData = frameData.Get<UniversalCameraData>();
                var descriptor = cameraData.cameraTargetDescriptor;
                // �֗p MSAA �a�[�x�iblit ��?��?�F�j
                descriptor.msaaSamples = 1;
                descriptor.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.None;

                Init(renderGraph, descriptor);
            }

            // ���� pass �I����?�n??render graph pass
            // ��?�o�p��������????������?�s�I�����B
            // "BlitColorPass" ��?�� pass �I����
            // ��?�I using �� C# �I��??�@���C�� C# ? IDisposable ?�ۓI��??���Ǘ������C�p����??��?���B
            // ��?�� using (...) {} ?�@??��?�C���??�p builder.Dispose()�C������?���e??�A??�A�������C?�B
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("BlitColorPass", out var passData))
            {
                // ?�旹���O?�����I?���㉺���C�?�����I Color RT �a Depth RT
                var resourceData = frameData.Get<UniversalResourceData>();

                // �R�� material ��ܗ����?�I�M���C���Ȏ��v?�s�d�u
                passData.material = null;
                // ?�擖�O����?�F?�t��? source
                passData.source = resourceData.activeColorTexture;
                // ?�u destination�F���O����?�I Blit ?��?�t��i�ʏ퐥?�p��?�s�@?���I???���j
                passData.destination = texture;

                // ���e source ?����??����?�B?�����m Render Graph�F?�� pass ��?��??���C�����s�\�ݑ����n����C���C?��?��������v���B
                builder.UseTexture(passData.source);
                // ?�u ?�o?����?�F��?��?��?�o�� destination�B�Q�� 0 ���w Render Target 0�i��A����??�F?�o���j�B
                builder.SetRenderAttachment(passData.destination, 0);

                // ?�u?������ SetRenderFunc
                // ?�u? pass �I�^��?�s??�����FExecutePass(...)�CExecutePass �ʏ��� Blitter.BlitTexture(rgContext.cmd, data.source, ..., data.material, ...)
                // => �� C# ���ILambda �\?��?�@�C�狩�g��?�����h�B
                // SetRenderFunc�F���e ����?�s?�� Pass �I�����iRenderGraph ���?�s?�i�^��?�p���j�B
                // ExecutePass�F?�w��I???�s??�C��@?�p Blitter.BlitTexture() ��
                builder.SetRenderFunc((PassData passData, RasterGraphContext rgContext) => ExecutePass(passData, rgContext));
            }
        }

        // ��??�� �� ?���� color buffer
        public void RecordBlitBackToColor(RenderGraph renderGraph, ContextContainer frameData)
        {
            // �@�� BlitData's texture �� invalid ?�����v�L�평�n�� ���� �� error ?����
            if (!texture.IsValid()) return;

            // ���� pass �I����?�n??render graph pass
            // ��?�o�p��������????������?�s�I�����B
            using (var builder = renderGraph.AddRasterRenderPass<PassData>($"BlitBackToColorPass", out var passData))
            {
                // ?�旹���O?�����I?���㉺���C�?�����I Color RT �a Depth RT
                var resourceData = frameData.Get<UniversalResourceData>();

                passData.material = null;
                passData.source = texture;
                passData.destination = resourceData.activeColorTexture;

                // ���e source ?����??����?�B
                builder.UseTexture(passData.source);
                // ?�u ?�o?����?�F��?��?��?�o�� destination�B�Q�� 0 ���w Render Target 0�i��A����??�F?�o���j�B
                builder.SetRenderAttachment(passData.destination, 0);

                // ?�u?������ SetRenderFunc
                builder.SetRenderFunc((PassData passData, RasterGraphContext rgContext) => ExecutePass(passData, rgContext));
            }
        }

        // BlitData ?���I�꘢���@�C�p�������O?�I RenderGraph �� �Y���꘢�S��?���I pass�C�g�p�񋟓I Material ?���?�s?���B
        public void RecordFullScreenPass(RenderGraph renderGraph, string passName, Material material)
        {
            // ??���ۛߗL���?�I?�o?�� (texture) �ȋy��?���ۗL��
            // �@�ʖv�L�A���� return�C?�h�~??��@�I GPU ?��?�v??�B
            if (!texture.IsValid() || material == null)
            {
                Debug.LogWarning("Invalid input texture handle, will skip fullscreen pass.");
                return;
            }

            // ?�n���e Raster Pass�B
            // ?���꘢ Raster ?�^�I RenderPass�B?�� Pass ��� GPU ��?�s?������i�ʏ�A���S���l?�`�j�B
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                // ��?�O�@?�t��(Ping-Pong Buffering)
                m_IsFront = !m_IsFront;

                // ?�u?�������B�g�p�V�O??�I texture ��??���B�g�p�񋟓I material ��?�s?���B
                passData.material = material;
                passData.source = texture;

                // ��??�� active texture�B��? m_IsFront ���f���O?�ʓ�?�꘢ RT Handle�B
                if (m_IsFront)
                    passData.destination = m_TextureHandleFront;
                else
                    passData.destination = m_TextureHandleBack;

                // .UseTexture() ��? RenderGraph�F��?�� pass ?�旹 source
                builder.UseTexture(passData.source);
                // .SetRenderAttachment() ��?���F��I color output �� destination�iBlit�I?�o��?�j
                builder.SetRenderAttachment(passData.destination, 0);

                // �X�V���O�I active ?�o?���B
                // ?��?�s���꘢ pass�C��?�s�c texture �w���ŐV�I output�B??���ꎟ?�p?�C?��I�A�?��?���ŋߍX�V?�I���e�B
                texture = passData.destination;

                builder.SetRenderFunc((PassData passData, RasterGraphContext rgContext) => ExecutePass(passData, rgContext));
            }
        }

        static void ExecutePass(PassData data, RasterGraphContext rgContext)
        {
            // Blitter.BlitTexture() �� Unity �����I�����S��?������
            if (data.material == null)
                // ����?�� source ����??���C�ٔC��?��
                Blitter.BlitTexture(rgContext.cmd, data.source, scaleBias, 0, false);
            else
                // �g�p material ? source ?�s?���@?�o����??��
                Blitter.BlitTexture(rgContext.cmd, data.source, scaleBias, data.material, 0);
        }

        // �a using ��p��?��??�p�I Dispose �s�����꘢
        public void Dispose()
        {
            m_TextureFront?.Release();
            m_TextureBack?.Release();
        }
    }

    // Initial render pass for the renderer feature which is run to initialize the data in frameData and copying
    // the camera's color attachment to a texture inside BlitData so we can do transformations using blit.
    class BlitStartRenderPass : ScriptableRenderPass
    {
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Creating the data BlitData inside frameData.
            var blitTextureData = frameData.Create<BlitData>();
            // Copies the camera's color attachment to a texture inside BlitData.
            blitTextureData.RecordBlitColor(renderGraph, frameData);
        }
    }

    // Render pass which makes a blit for each material given to the renderer feature.
    class BlitRenderPass : ScriptableRenderPass
    {
        List<Material> m_Materials;

        // Setup function used to retrive the materials from the renderer feature.
        public void Setup(List<Material> materials)
        {
            m_Materials = materials;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Retrives the BlitData from the current frame.
            var blitTextureData = frameData.Get<BlitData>();
            foreach(var material in m_Materials)
            {
                // Skip current cycle if the material is null since there is no need to blit if no
                // transformation happens.
                if (material == null) continue;
                // Records the material blit pass.
                blitTextureData.RecordFullScreenPass(renderGraph, $"Blit {material.name} Pass", material);
            }    
        }
    }

    // Final render pass to copying the texture back to the camera's color attachment.
    class BlitEndRenderPass : ScriptableRenderPass
    {
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Retrives the BlitData from the current frame and blit it back again to the camera's color attachment.
            var blitTextureData = frameData.Get<BlitData>();
            blitTextureData.RecordBlitBackToColor(renderGraph, frameData);
        }
    }

    [SerializeField]
    [Tooltip("Materials used for blitting. They will be blit in the same order they have in the list starting from index 0. ")]
    List<Material> m_Materials;

    BlitStartRenderPass m_StartPass;
    BlitRenderPass m_BlitPass;
    BlitEndRenderPass m_EndPass;

    // Here you can create passes and do the initialization of them. This is called everytime serialization happens.
    public override void Create()
    {
        m_StartPass = new BlitStartRenderPass();
        m_BlitPass = new BlitRenderPass();
        m_EndPass = new BlitEndRenderPass();

        // Configures where the render pass should be injected.
        m_StartPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        m_BlitPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        m_EndPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Early return if there is no texture to blit.
        if (m_Materials == null || m_Materials.Count == 0) return;

        // Pass the material to the blit render pass.
        m_BlitPass.Setup(m_Materials);

        // Since they have the same RenderPassEvent the order matters when enqueueing them.
        renderer.EnqueuePass(m_StartPass);
        renderer.EnqueuePass(m_BlitPass);
        renderer.EnqueuePass(m_EndPass);
    }
}


