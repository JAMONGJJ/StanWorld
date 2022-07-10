using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace stanworld
{
    using network;
    using ChatMsgView;
    using ChatPartyModel;
    using PrivateChatModel;

    public enum ParserType
    {
        None = 0,
        SFStoMODEL,
        MODELtoVIEW,
        VIEWtoMODEL,
        MODELtoSFS,
        SFStoVIEW,
        VIEWtoSFS,
    }

    public interface IParseInputData
    {

    }
    public interface IParseOutputData
    {

    }
    public class RequestReply
    {
        public long RequestId;
    }
    public interface ISfsData : IParseOutputData, IParseInputData
    {

    }
    public interface IModelData : IParseOutputData, IParseInputData
    {

    }
    public interface IViewData : IParseOutputData, IParseInputData
    {

    }

    public interface IParseHelper
    {

    }
    public interface IModelParseHelper : IParseHelper
    {

    }



    public class ParsingManager
    {
        private Dictionary<ParserType, Parser> parserList { get; set; } = new Dictionary<ParserType, Parser>();

        public ParsingManager() { }

        public void Add(ParserType type, Parser parser)
        {
            try
            {
                parserList.Add(type, parser);
            }
            catch (Exception e)
            {
                LogFileWriter.LogError(e.ToString(), LogFileWriter.LogType.EXCEPTION);
            }
        }

        public void Clear()
        {
            parserList.Clear();
        }

        public bool ContainsKey(ParserType type)
        {
            if (parserList.ContainsKey(type))
            {
                return true;
            }
            return false;
        }

        public void Remove(ParserType type)
        {
            if (ContainsKey(type))
            {
                parserList.Remove(type);
            }
        }

        public IParseOutputData Parse(ParserType type, string key, IParseInputData data, IParseHelper helper = null)
        {
            try
            {
                if (parserList.ContainsKey(type))
                {
                    return parserList[type].Parse(key, data, helper);
                }
                else
                {
                    throw new Exception("There is no [type] exists in the parser map!");
                }
            }
            catch (Exception e)
            {
                LogFileWriter.LogError(e.ToString(), LogFileWriter.LogType.EXCEPTION);
                return null;
            }
        }
    }

    public class Parser
    {
        public delegate IParseOutputData ParsingDelegate(IParseInputData _data, IParseHelper _helper = null);
        public Dictionary<string, ParsingDelegate> ParsingDelegateContainer;

        public virtual void InitParsingDelegateContainer()
        {
            LogFileWriter.Log("PARSER", LogFileWriter.LogType.CONTROLLER_INIT);
            ParsingDelegateContainer = new Dictionary<string, ParsingDelegate>();
        }

        public virtual void AddDelegate(string _key, ParsingDelegate _delegate)
        {
            try
            {
                if (ParsingDelegateContainer.ContainsKey(_key))
                {
                    throw new Exception("You cannot add value with same key!");
                }
                else
                {
                    ParsingDelegateContainer.Add(_key, _delegate);
                }
            }
            catch (Exception e)
            {
                LogFileWriter.LogError(e.ToString(), LogFileWriter.LogType.EXCEPTION);
            }
        }

        public virtual void AddDelegates(string _key, List<ParsingDelegate> _delegates)
        {
            foreach (ParsingDelegate d in _delegates)
            {
                AddDelegate(_key, d);
            }
        }

        public virtual IParseOutputData Parse(string _key, IParseInputData _data, IParseHelper _helper = null)
        {
            try
            {
                if (ParsingDelegateContainer.ContainsKey(_key))
                {
                    return ParsingDelegateContainer[_key](_data, _helper);
                }
                else
                {
                    throw new Exception("There is no [key] exists in the container!");
                }
            }
            catch (Exception e)
            {
                LogFileWriter.LogError(e.ToString(), LogFileWriter.LogType.EXCEPTION);
                return null;
            }
        }
    }

    public class SfsToModelParser : Parser { }

    public class ModelToSfsParser : Parser { }

    public class ModelToViewParser : Parser { }

    public class ViewToModelParser : Parser { }

    public class SfsToViewParser : Parser { }

    public class ViewToSfsParser : Parser { }
}

