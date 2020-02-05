﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using Client.Desktop.ViewModels.Common.Attributes;

namespace Client.Desktop.Views
{
    public static class Helper
    {
        private static List<Type> _types;

        /// <summary>
        /// Get types when contains attribute.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="attributeType">The attribute.</param>
        /// <returns>The collection of types.</returns>
        public static IEnumerable<Type> GetTypesWithAttribute(Assembly assembly, Type attributeType)
        {
            return assembly.GetTypes().Where(type => type.GetCustomAttributes(attributeType, true).Any());
        }

        /// <summary>
        /// Get control by <see cref="HasViewModelAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="FrameworkElement"/>.</typeparam>
        /// <param name="viewModelType">The view model type.</param>
        /// <returns>The <see cref="FrameworkElement"/>.</returns>
        public static T GetControl<T>(this Type viewModelType) where T : FrameworkElement
        {
            if (_types == null)
            {
                _types = new List<Type>();
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())//.Where(x => x.FullName.Contains("PALMS")))
                {
                    var types = GetTypesWithAttribute(assembly, typeof(HasViewModelAttribute)).ToList();
                    if (types.Any())
                        _types.AddRange(types);
                }

            }

            var elementType = _types.FirstOrDefault(t =>
            {
                var attribute = t.GetCustomAttribute<HasViewModelAttribute>();

                return attribute != null && attribute.ViewModelType == viewModelType;
            });

            if (elementType != null)
                return Activator.CreateInstance(elementType) as T;

            return null;
        }

        /// <summary>
        /// Run action in main thread.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="dispatcherPriority">The disptcher priority.</param>
        public static void RunInMainThread(this Action action, DispatcherPriority dispatcherPriority = DispatcherPriority.Normal)
        {
            Application.Current?.Dispatcher.Invoke(dispatcherPriority, action);
        }


        /// <summary>
        /// Create <see cref="ObservableCollection{T}"/>
        /// </summary>
        /// <typeparam name="T">The collection parameter.</typeparam>
        /// <param name="enumerable">The items of collection.</param>
        /// <returns>The <see cref="ObservableCollection{T}"/></returns>
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
            => new ObservableCollection<T>(enumerable);

        /// <summary>
        /// Performs the specified action on each element of the specified collection.
        /// </summary>
        /// <typeparam name="T">The type of enumerable element.</typeparam>
        /// <param name="enumerable">The collections of elements.</param>
        /// <param name="action">The <see cref="Action"/> to perform on each element of collection./></param>
        /// <exception cref="ArgumentNullException">enumerable is null. -or- action is null.</exception>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            if (action == null) throw new ArgumentNullException(nameof(action));

            foreach (var item in enumerable) action(item);
        }

        /// <summary>
        /// Add items to collectino.
        /// </summary>
        /// <typeparam name="T">The type of collection item.</typeparam>
        /// <param name="enumerable">The target collection.</param>
        /// <param name="items">The items.</param>
        public static void AddRange<T>(this ObservableCollection<T> enumerable, IEnumerable<T> items)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            if (items == null) throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
                enumerable.Add(item);
        }

        public static string RemoveDoubleSpace(this string text)
        {
            return Regex.Replace(text, @"\s+", " ");
        }

        //public static BitmapImage GetBitmapImage(byte[] value)
        //{
        //    if (value == null || value.Length == 0) return null;
        //    var image = new BitmapImage();
        //    using (var mem = new MemoryStream(value))
        //    {
        //        mem.Position = 0;
        //        image.BeginInit();
        //        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
        //        image.CacheOption = BitmapCacheOption.OnLoad;
        //        image.UriSource = null;
        //        image.StreamSource = mem;
        //        image.EndInit();
        //    }
        //    image.Freeze();
        //    return image;
        //}

        //public static byte[] GetByteArray(string imagePath)
        //{
        //    if (imagePath == null)
        //        return null;

        //    byte[] imageByteArray = null;
        //    FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
        //    using (BinaryReader reader = new BinaryReader(fileStream))
        //    {
        //        imageByteArray = new byte[reader.BaseStream.Length];
        //        for (int i = 0; i < reader.BaseStream.Length; i++)
        //            imageByteArray[i] = reader.ReadByte();
        //    }
        //    return imageByteArray;
        //}

        //public static Bitmap GetBitmap(BitmapImage bitmapImage)
        //{
        //    if (bitmapImage == null)
        //        return null;

        //    using (MemoryStream outStream = new MemoryStream())
        //    {
        //        BitmapEncoder enc = new BmpBitmapEncoder();
        //        enc.Frames.Add(BitmapFrame.Create(bitmapImage));
        //        enc.Save(outStream);
        //        System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

        //        return new Bitmap(bitmap);
        //    }
        //}
    }
}
