﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;

using SerializeMachine.Utility;

namespace SerializeMachine.Core
{
    public sealed class Serializator
    {
        internal const string XML_ELEMENTNAME_NULL = "NULL";
        internal const string XML_ATTRIBUTENAME_GUID = "GUID";
        internal static readonly Guid GUID_NULL = Guid.Empty;
        internal static readonly string GUID_NULL_TOSTRING = GUID_NULL.ToString();

        public HeapManager HeapManager;
        public TypeManager TypeManager;
        public ResolverBank ResolverBank;

        public void FlashHeaps()
        {
            HeapManager.FlashHeaps();
        }

        public object Deresolve(XElement serialized)
        {
            if (XMLUtility.IsNullOf(serialized))
            {
                return null;
            }
            else
            {
                var conventionType = serialized.Name.LocalName;
                return DeresolveInternal(serialized, conventionType);
            }
            

        }
        /// <summary>
        /// Десериализирует xml-узел serializedObject в возвращаймый объект, используя действительный сериализатор для типа объекта serializedObject.
        /// Не допускается значение null ни для какаих входных параметров.
        /// Метод является контекстным.
        /// Обращается к банку resolver`ов для получения resolver`а 
        /// </summary>
        /// <param name="serializedObject">Десериализируемый объект</param>
        /// <param name="convention">Условное обозночение типа</param>
        /// <returns>Результат десериализации</returns>
        internal object DeresolveInternal(XElement serializedObject, string convention)
        {
            return ResolverBank.GetResolver(convention).Deserialzie(serializedObject);
        }

        /// <summary>
        /// Десериализирует xml-узел serializedObject в возвращаймый объект, используя указанный resolver.
        /// Не допускается значение null ни для какаих входных параметров.
        /// resolver должен соответствовать типу десериализирумого объекта.
        /// Метод является внеконтекстным, однако указаный в нем resolver может обращаться к контексту сериализатора (куче,словарю типов и тд).
        /// </summary>
        /// <param name="serializedObject">Десериализируемый объект</param>
        /// <param name="resoler">Resolver осуществляющий десериализацию</param>
        /// <returns>Результат десериализации</returns>
        internal object DeresolveInternal(XElement serializedObject, IResolver resolver)
        {
            return resolver.Deserialzie(serializedObject);
        }

        /// <summary>
        /// Сериализирует объект resolveObject в возвращаемый xml-узел, используя действительный сериализатор для типа объекта resolveObject
        /// Допускается значение null для resolveObject, в данном случае будет возвращен пустой xml-узел с именем соответствующему имени нулевого объекта  
        /// Метод является контекстным. 
        /// Обрашается к менеджеру типов для определения условного обозноечения типа.
        /// </summary>
        /// <param name="resolveObject">Сериализируемый объект</param>
        /// <returns>xml-узел являющейся результатом сериализации</returns>
        public XElement Resolve(object resolveObject)
        {
            if (resolveObject == null)
                return TypeDictionary.GetNullHead();
            else
            {
                var conventionType = TypeManager.ConventionOf(resolveObject.GetType());
                var serialized = TypeDictionary.GetHead(conventionType);
                ResolveToInternal(resolveObject, conventionType, serialized);
                return serialized;
            }
        }
        internal XElement ResolveInternal(object resolveObject,string convention, IResolver resolver)
        {
            if (resolveObject == null)
                return TypeDictionary.GetNullHead();
            else
            {
                var serialized = TypeDictionary.GetHead(convention);
                ResolveToInternal(resolveObject, resolver, serialized);
                return serialized;
            }
        }
        internal XElement ResolveInternal(object resolveObject, string convention)
        {
            if (resolveObject == null)
                return TypeDictionary.GetNullHead();
            else
            {
                var serialized = TypeDictionary.GetHead(convention);
                ResolveToInternal(resolveObject, convention, serialized);
                return serialized;
            }
        }
        
        /// <summary>
        /// Сериализирует объект resolveObject в xml-узел serializeHere используя действительный сериализатор для объектов типа conventionType
        /// Не допускается значение null ни для какаих входных параметров.
        /// Метод является контекстным. 
        /// Обращается к банку resolver`ов для получения resolver`а 
        /// </summary>
        /// <param name="resolveObject">Сериализируемый объект</param>
        /// <param name="conventionType">Условное обозночение типа</param>
        /// <param name="serializeHere">xml-узел в который будет записан результат сериализации</param>
        internal void ResolveToInternal(object resolveObject, string conventionType, XElement serializeHere)
        {
            ResolverBank.GetResolver(conventionType).Serialize(serializeHere, resolveObject);
        }

        /// <summary>
        /// Сериализирует объект resolveObject в xml-узел serializeHere используя указанный resolver.
        /// Не допускается значение null ни для какаих входных параметров.
        /// resolver должен соответствовать типу сериализирумого объекта (resolveObject).
        /// Метод является внеконтекстным, однако указаный в нем resolver может обращаться к контексту сериализатора (куче,словарю типов и тд)
        /// </summary>
        /// <param name="resolveObject">Сериализируемый объект</param>
        /// <param name="resoler">Resolver осуществляющий сериализацию</param>
        /// <param name="serializeHere">xml-узел в который будет записан результат сериализации</param>
        internal void ResolveToInternal(object resolveObject, IResolver resoler, XElement serializeHere)
        {
            resoler.Serialize(serializeHere, resolveObject);
        }

        public XElement ContextResolve(object resolveObject)
        {
            if (resolveObject == null)
                return XMLUtility.CreateNullNode();

            var type = resolveObject.GetType();//Запоминаем тип //TODO REMOVE THIS
            var conventionType = TypeManager.ConventionOf(type);//Получаем условное обозночение типа
            //Для начала пытаемся понять с чем мы вообще работаем 
            //TODO WORKING WITH TYPE MANAGER!
            if (SerializationUtility.Targeting.IsSaveReferenceInternal(type))
            {
                Guid finalGuid;
                //Метод GetCreateGuid(object,out Guid) возвращает bool значение которого отвечает на вопрос: 
                //Был ли создан новый Guid для этого объекта
                if(HeapManager.GetCreateGuid(resolveObject,out finalGuid))
                    //Для объекта resolveObject уже создан Guid методом GetCreateGuid
                    //Поэтому теперь мы обновляем сериализированное состояние объекта и добавляем его в кучу 
                    //сериалихированных объектов
                    HeapManager.Serialized.Push(finalGuid, ResolveInternal(resolveObject,conventionType));

                //В случае если объект resolveObject существует в куче, предпологается что он также 
                //существет и в куче сериализированных объектов.
                //В конечном счете, в итоговой xml-узел, в качестве значения, будет помещен guid указывающий на объект resolveObject
                return XMLUtility.CreateReferenceNode(conventionType, finalGuid.ToString());
            }
            return ResolveInternal(resolveObject, conventionType);
            
        }
        public object ContextDeresolve(XElement serializedObject)
        {
            if (XMLUtility.IsNullOf(serializedObject))
                return null;

            var conventionType = serializedObject.Name.LocalName;
            var type = TypeManager.TypeOf(conventionType);

            if (SerializationUtility.Targeting.IsSaveReferenceInternal(type))
            {
                var guid = XMLUtility.GuidOf(serializedObject);
                if (guid == Serializator.GUID_NULL) return null;
                var obj = HeapManager.GetObject(guid);
                if (obj == null)
                {
                    obj = DeresolveInternal(serializedObject, conventionType);
                    HeapManager.Original.AddObject(obj, guid);
                    //HeapManager.Serialized.Push(guid, serializedObject);
                }
                return obj;
            }
            return DeresolveInternal(serializedObject, conventionType);
        }

        /*
        public XElement GetFieldSerialized(object obj)
        {
            var type = obj.GetType();
            var convention = TypeManager.ConventionOf(type);
            var result = TypeDictionary.GetHead(convention);

            if (SerializationUtility.Targeting.IsSaveReferenceInternal(type))
            {
                Guid guid;
                if (HeapManager.GetCreateGuid(obj, out guid))
                    HeapManager.Serialized.Push(guid, Resolve(obj));
                result.Value = guid.ToString();
            }
            else
            {
                ResolveToInternal(obj, convention, result);
            }

            return result;
        }
        public object GetFieldDeserialized(XElement serializedObject)
        {
            var convention = serializedObject.Name.LocalName;

            if (SerializationUtility.Targeting.IsSaveReferenceInternal(TypeManager.TypeOf(convention)))
            {
                Guid guid;
                if (HeapManager.GetCreateGuid(obj, out guid))
                    HeapManager.Serialized.Push(guid, Resolve(obj));
                result.Value = guid.ToString();
            }
            else
            {
                ResolveToInternal(obj, convention, result);
            }

            return result;
        }
        */

        public Serializator()
        {
            TypeManager = new global::SerializeMachine.TypeManager();
            ResolverBank = new global::SerializeMachine.ResolverBank(this);
            HeapManager = new HeapManager(this);
        }
    }
}
