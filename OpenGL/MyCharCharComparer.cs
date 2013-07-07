using System.Collections.Generic;

namespace GraphicLib.OpenGL
{
  internal class MyCharCharComparer : IEqualityComparer<KeyValuePair<char, char>>
  {
    #region Implementation of IEqualityComparer<in KeyValuePair<char,char>>

    /// <summary>
    /// Определяет, равны ли два указанных объекта.
    /// </summary>
    /// <returns>
    /// Значение true, если указанные объекты равны; в противном случае — значение false.
    /// </returns>
    /// <param name="x">Первый сравниваемый объект типа <paramref name="T"/>.</param><param name="y">Второй сравниваемый объект типа <paramref name="T"/>.</param>
    public bool Equals(KeyValuePair<char, char> x, KeyValuePair<char, char> y)
    {
      return (x.Key == y.Key && x.Value == y.Value);
    }

    /// <summary>
    /// Возвращает хэш-код указанного объекта.
    /// </summary>
    /// <returns>
    /// Хэш-код указанного объекта.
    /// </returns>
    /// <param name="obj">Объект <see cref="T:System.Object"/>, для которого должен быть возвращен хэш-код.</param><exception cref="T:System.ArgumentNullException">Тип <paramref name="obj"/> является ссылочным типом, значение параметра <paramref name="obj"/> — null.</exception>
    public int GetHashCode(KeyValuePair<char, char> obj)
    {
      /*int hashCode = obj.Key.GetHashCode() ^ obj.Value.GetHashCode();
      return hashCode.GetHashCode();*/
      return obj.GetHashCode();
    }

    #endregion
  }

}
