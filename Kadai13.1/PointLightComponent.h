﻿// ----------------------------------------------------------------
// From Game Programming in C++ by Sanjay Madhav
// Copyright (C) 2017 Sanjay Madhav. All rights reserved.
// 
// Released under the BSD License
// See LICENSE in root directory for full details.
// ----------------------------------------------------------------

#pragma once
#include "Math.h"
#include "Component.h"

class PointLightComponent : public Component
{
public:
	PointLightComponent(class Actor* owner);
	~PointLightComponent();

	// Draw this point light as geometry
	void Draw(class Shader* shader, class Mesh* mesh);

	// Diffuse color
	Vector3 mDiffuseColor;
	// Specular color
	Vector3 mSpecularColor;
	// Radius of light
	float mInnerRadius;
	float mOuterRadius;
};