using Sfs2X.Entities.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace stanworld
{
    namespace network
    {

        public class ResponseListenerList
        {
            public Dictionary<string, Dictionary<int, ResponseListenerDelegate>> ListenerList = new Dictionary<string, Dictionary<int, ResponseListenerDelegate>>();

            public void AddListener(string cmd, ResponseListenerDelegate listener, int index = 0)
            {
                try
                {
                    if (ListenerList.ContainsKey(cmd))
                    {
                        if (ListenerList[cmd].ContainsKey(index))
                        {
                            throw new Exception("You cannot add the same key!");
                        }
                        else
                        {
                            ListenerList[cmd].Add(index, listener);
                        }
                    }
                    else
                    {
                        Dictionary<int, ResponseListenerDelegate> Listeners = new Dictionary<int, ResponseListenerDelegate>();
                        Listeners.Add(index, listener);
                        ListenerList.Add(cmd, Listeners);
                    }
                }
                catch (Exception e)
                {
                    LogFileWriter.LogError(e.ToString(), LogFileWriter.LogType.EXCEPTION);
                }
            }

            public bool ContainsKey(string key)
            {
                if (ListenerList.ContainsKey(key))
                {
                    return true;
                }
                return false;
            }

            public void InvokeListener(string cmd, int index, SFSObject packet)
            {
                try
                {
                    if (ListenerList.ContainsKey(cmd))
                    {
                        if (ListenerList[cmd].ContainsKey(index))
                        {
                            ListenerList[cmd][index](packet);
                        }
                        else
                        {
                            throw new Exception("This index doesn't exist in the listener container!");
                        }
                    }
                    else
                    {
                        throw new Exception("This command doesn't have any listeners!");
                    }
                }
                catch (Exception e)
                {
                    LogFileWriter.LogError(e.ToString(), LogFileWriter.LogType.EXCEPTION);
                }
            }
        }
    }
}
