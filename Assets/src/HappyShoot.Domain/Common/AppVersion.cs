namespace HappyShoot.Domain.Common
{
    /// <summary>
    /// Master Application Version Registry.
    /// 형님께서 직접 버전을 수정/관리하시는 공식 단일 소스입니다!
    /// </summary>
    public static class AppVersion
    {
        /// <summary>
        /// 현재 앱 버전 (형님이 필요하실 때 이 값을 직접 변경하시면 게임 전체에 자동 반영됩니다)
        /// </summary>
        public const string Current = "v0.3.11";

        /// <summary>
        /// 빌드/업데이트 날짜
        /// </summary>
        public const string ReleaseDate = "2026-08-22";

        /// <summary>
        /// 정식 버전 표기 문자열
        /// </summary>
        public static string VersionText => $"HappyShoot {Current}";
        public static string FullVersionText => $"HappyShoot {Current} ({ReleaseDate})";
    }
}
