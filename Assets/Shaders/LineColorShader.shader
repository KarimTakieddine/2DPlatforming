Shader "Unlit/LineColorShader"
{
	Properties
	{
		_BackgroundColor("Background Color", Color) = (1, 1, 1, 1)
		_ColorStep("Color Step", Range(0.0, 180.0))	= 0.0
	}

	/*
		A Shader can contain one or more SubShaders, which are primarily used to implement shaders
		for different GPU capabilities.
		
		All the SubShaders should present similar results using different techniques for each
		architecture. ShaderLab translates the code of the shader automatically to other
		architectures.
	*/
	
	SubShader
	{
		/*
			Tags are a way of telling Unity certain properties of the shader we are writing.
			For instance, the order in which it should be rendered (Queue) and how it should
			be rendered (RenderType).

			When rendering triangles, the GPU usually sorts them according to their distance
			from the camera, so that the further ones are drawn first. This is typically enough
			to render solid geometries, but it often fails with transparent objects.
			
			This is why Unity allows to specify the tag Queue which gives control on the rendering
			order of each material. Queue accepts integer positive numbers (the smaller it is, the
			sooner is drawn); mnemonic labels can also be used:

				Background (1000): used for backgrounds and skyboxes,

				Geometry (2000): the default label used for most solid objects,

				Transparent (3000): used for materials with transparent properties, such glass, fire, particles and water;

				Overlay (4000): used for effects such as lens flares, GUI elements and texts.
		*/

		Tags
		{ 
			"RenderType"	= "Opaque"
			"Queue"			= "Geometry"
		}

		/*
			Each SubShader is composed of a number of passes, and each Pass represents an execution
			of the Vertex and Fragment code for the same object rendered with the Material of the Shader.
		*/
		
		Pass
		{
			/*
				These directives define in ShaderLab the language used for the shader. Unity can work with the
				shader languages of Cg and GLSL.the Cg language is recommended, because several optimization
				steps are implemented for the different architectures.
			*/

			CGPROGRAM
			#pragma vertex		vertexShader	// compile function vertexShader() as the vertex shader.
			#pragma fragment	fragmentShader	// compile function fragmentShader() as the fragment shader.

			struct Vertex
			{
				float4 position : POSITION;
			};

			struct ScreenVertex
			{
				float4 position : SV_POSITION;
			};

			ScreenVertex vertexShader(Vertex input)
			{
				ScreenVertex output;
				output.position = UnityObjectToClipPos(input.position); // mul(UNITY_MATRIX_MVP, input.position); - Removed by ShaderLab compiler
				return output;
			}

			half4 _BackgroundColor;
			float _ColorStep;
			float value = 0.0;

			half4 fragmentShader() : COLOR
			{
				value += _Time.x * _ColorStep;
				float sine = clamp(sin(value), 0.0, 1.0);
				return half4(_BackgroundColor.x * sine, _BackgroundColor.y * sine, _BackgroundColor.z * sine, _BackgroundColor.w);
			}
			ENDCG
		}
	}
}