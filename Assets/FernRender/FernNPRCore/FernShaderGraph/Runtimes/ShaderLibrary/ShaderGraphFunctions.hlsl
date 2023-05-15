
void GetMainLightShadow_half(out half shadow)
{
    shadow = GetMainLight().shadowAttenuation;
   // shadow = 1;
}

void GetMainLightShadow_float(out half shadow)
{
    shadow = GetMainLight().shadowAttenuation;
}