﻿#version 450 core

#define BIAS 0.1

void main()
{             
    gl_FragDepth = gl_FragCoord.z;
    gl_FragDepth += gl_FrontFacing ? BIAS : 0.0;
}