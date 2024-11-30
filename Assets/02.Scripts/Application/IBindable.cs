namespace JaehyeokSong0.Tacidto.Application
{
    /// <summary>
    /// View와 Data를 바인딩하여 reactive하게 사용하기 위한 인터페이스
    /// </summary>
    public interface IBindable
    {
        void Bind();
        void Unbind();
    }
}