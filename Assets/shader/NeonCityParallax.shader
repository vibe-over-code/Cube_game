Shader "Custom/ProceduralNeonCity"
{
    Properties
    {
        // Базовые цвета неона
        _NeonColor1 ("Neon Cyan", Color) = (0, 1, 1, 1)
        _NeonColor2 ("Neon Pink", Color) = (1, 0, 0.5, 1)
        _SkyColor ("Sky Top", Color) = (0.02, 0.02, 0.05, 1)
        _GroundColor ("Sky Bottom", Color) = (0.1, 0.05, 0.1, 1)
        
        // Настройки движения и масштаба
        _BaseSpeed ("Base Scroll Speed", Float) = 0.1
        _BaseDensity ("City Density", Float) = 10.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _NeonColor1;
            float4 _NeonColor2;
            float4 _SkyColor;
            float4 _GroundColor;
            float _BaseSpeed;
            float _BaseDensity;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Функции случайности (хеширование)
            float Hash11(float p) {
                p = frac(p * .1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            float2 Hash22(float2 p) {
                float3 p3 = frac(float3(p.xyx) * float3(.1031, .1030, .0973));
                p3 += dot(p3, p3.yzx+33.33);
                return frac((p3.xx+p3.yz)*p3.zy);
            }

            // Функция отрисовки ОДНОГО слоя зданий
            float3 DrawCityLayer(float2 uv, float speed, float density, float3 color, float zIndex) {
                float time = _Time.y * speed;
                float2 st = uv * float2(density, 1.0);
                st.x += time; // Движение слоя

                float id = floor(st.x);
                float2 ipos = Hash22(float2(id, zIndex)); // Рандом для здания

                // 1. Форма здания
                // Рандомная высота и ширина
                float height = ipos.x * 0.6 + 0.1; 
                float width = ipos.y * 0.3 + 0.6;   // От 0.6 до 0.9 ширины ячейки

                float2 localUV = frac(st);
                // Центрируем здание в ячейке
                float buildingMask = step(abs(localUV.x - 0.5), width * 0.5);
                buildingMask *= step(localUV.y, height);

                // 2. Антенны/шпили на крыше
                float spireWidth = 0.02 * (1.0 + ipos.y);
                float spireHeight = height + ipos.x * 0.15;
                float spires = step(abs(localUV.x - (0.3 + ipos.y * 0.4)), spireWidth); // Сдвиг шпиля
                spires *= step(localUV.y, spireHeight);
                
                buildingMask = max(buildingMask, spires);

                // 3. Окна
                float3 finalColor = float3(0,0,0);
                if (buildingMask > 0.0) {
                    // Базовый темный цвет стены здания
                    float3 wallColor = color * 0.2 * (zIndex + 0.5); 
                    finalColor = wallColor;

                    // Сетка окон
                    float2 winUV = localUV * float2(15.0, 30.0);
                    float2 winID = floor(winUV);
                    float2 winRand = Hash22(winID + id * 10.0); // Рандом для окна

                    // Рисуем окна, только если мы внутри основного тела здания (не шпиля)
                    if (localUV.y < height - 0.02 && abs(localUV.x - 0.5) < width * 0.5 - 0.02) {
                        // Окно существует, если frac (зазор) маленький, и рандом > 0.3 (выключенные окна)
                        float windows = step(0.1, frac(winUV.x)) * step(0.1, frac(winUV.y));
                        windows *= step(0.3, winRand.x);
                        
                        // Свечение окон (немного мигает)
                        float flicker = sin(time * 2.0 + winRand.y * 10.0) * 0.1 + 0.9;
                        finalColor = lerp(wallColor, color * flicker * 2.0, windows);
                    }

                    // 4. Неоновая обводка крыши
                    float roofEdge = step(height - 0.01, localUV.y) * step(localUV.y, height) * step(abs(localUV.x-0.5), width*0.5);
                    finalColor += roofEdge * color * 3.0;
                }

                return finalColor;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // 1. Фон - градиент неба
                float3 sky = lerp(_GroundColor.rgb, _SkyColor.rgb, uv.y);
                float3 finalCol = sky;

                // 2. Слой 3 (Самый дальний, медленный, плотный, темный)
                float3 layer3 = DrawCityLayer(uv, _BaseSpeed * 0.2, _BaseDensity * 2.5, _NeonColor2.rgb * 0.3, 3.0);
                if(length(layer3) > 0.0) finalCol = layer3;

                // 3. Слой 2 (Средний)
                float3 layer2 = DrawCityLayer(uv, _BaseSpeed * 0.5, _BaseDensity * 1.5, _NeonColor1.rgb * 0.6, 2.0);
                // Простой альфа-блендинг: если пиксель здания не черный, рисуем его
                if(length(layer2) > 0.0) finalCol = layer2;

                // 4. Слой 1 (Самый ближний, быстрый, яркий)
                float3 layer1 = DrawCityLayer(uv, _BaseSpeed, _BaseDensity, _NeonColor1.rgb, 1.0);
                if(length(layer1) > 0.0) finalCol = layer1;

                // Добавим немного дымки/атмосферы снизу
                finalCol = lerp(finalCol, _GroundColor.rgb, (1.0 - uv.y) * 0.3);

                return fixed4(finalCol, 1.0);
            }
            ENDCG
        }
    }
}