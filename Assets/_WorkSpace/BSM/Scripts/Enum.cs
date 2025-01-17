using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum UnitType
{
    NONE, WATER, FIRE, GRASS, GROUND, ELECTRIC
}

[System.Serializable]
public enum BuffType
{
    NONE,
    RUSH = 1 << 11,
    BREAK_THROUGH = 1 << 10,
    GUARD = 1 << 8,
    SHELLING = 1 << 9, 
    DEFENSE = 1 << 7,
    SUPPORT = 1 << 5,
}

[System.Serializable]
public enum SortType
{
    NONE, LEVEL, POWERLEVEL, ENHANCELEVEL, OFFENSIVEPOWER, DEFENSEIVEPOWER, HEALTH
}

[System.Serializable]
public enum InfoTabType
{
    DETAIL, ENHANCE, EVOLUTION, NONE
}

/// <summary>
/// 캐릭터의 속성 enum입니다<br/>
/// 데이터 파싱에 사용되고 있으니 변경 필요시 보고해주세요
/// </summary>
[System.Serializable]
public enum ElementType
{
    /// <summary>
    /// 0: 무속성/미정의
    /// </summary>
    NONE,
    /// <summary>
    /// 1: 화룡
    /// </summary>
    FIRE,
    /// <summary>
    /// 2: 수룡
    /// </summary>
    WATER,
    /// <summary>
    /// 3:정룡 ?
    /// </summary>
    WIND,
    /// <summary>
    /// 4: 토룡
    /// </summary>
    EARTH,
    /// <summary>
    /// 5: 진룡
    /// </summary>
    METAL, 
}

/// <summary>
/// 캐릭터의 용맥 타입 enum입니다<br/>
/// 데이터 파싱에 사용되고 있으니 변경 필요시 보고해주세요
/// </summary>
[System.Serializable]
public enum DragonVeinType
{
    NONE, SINGLE, MULTI,
}

/// <summary>
/// 캐릭터의 역할군 enum입니다<br/>
/// 데이터 파싱에 사용되고 있으니 변경 필요시 보고해주세요
/// </summary>
[System.Serializable]
public enum RoleType
{
    NONE, ATTACKER, DEFENDER, SUPPORTER, 
}

[System.Serializable]
public enum EnhanceTokenType
{
    NONE, CHARACTER_TOKEN, COMMON_TOKEN, AUTO
}

[System.Serializable]
public enum EnhanceType
{
    NONE, ENHANCE, MILEAGE
}