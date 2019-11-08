﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Defines the <see cref="ServerUniversalSettings" />
/// </summary>
[Serializable]
public class ServerUniversalSettings : MonoBehaviour
{
    /// <summary>
    /// Defines the x_ObjectHelper
    /// </summary>
    internal x_ObjectHelper x_ObjectHelper;

    /// <summary>
    /// These are the enums we use to reference each variable
    /// </summary>
    public enum var_names
    {
        /// <summary>
        /// nothing... never used
        /// </summary>
        none,

        /// <summary>
        /// variable a
        /// </summary>
        f_a,

        /// <summary>
        /// variable b
        /// </summary>
        f_b,

        /// <summary>
        /// variable c... etc
        /// </summary>
        f_c,

        /// <summary>
        /// Defines the f_d
        /// </summary>
        f_d,

        /// <summary>
        /// Defines the f_x
        /// </summary>
        f_x,

        /// <summary>
        /// Defines the f_m
        /// </summary>
        f_m
    }

    /// <summary>
    /// Defines the <see cref="var_data" />
    /// </summary>
    [Serializable]
    public class var_data
    {
        /// <summary>
        /// This is what we see in the server_vars.json, I didnt want this to be string as it opens up to typos later
        /// </summary>
        public var_names ID;

        /// <summary>
        /// The actual value of the variable e.g: 1.1 critical chance
        /// </summary>
        public float value;

        /// <summary>
        /// Only here so we can have a clear description in the json file as we cannot have comments in a json file
        /// </summary>
        public string description;

        /// <summary>
        /// Initializes a new instance of the <see cref="var_data"/> class.
        /// </summary>
        /// <param name="var_name">The var_name<see cref="var_names"/></param>
        /// <param name="value">The value<see cref="float"/></param>
        /// <param name="description">The description<see cref="string"/></param>
        public var_data(var_names var_name, float value, string description)
        {
            this.ID = var_name;
            this.value = value;
            this.description = description;
        }
    }

    /// <summary>
    /// This is the dictionary that will store all our variables, note var_data also has var_names in it and should not be used at all.. yes, its weird but thats what I could come up with
    /// </summary>
    public Dictionary<var_names, var_data> dict_vars;

    //used to serialize/deserialize
    
    /// <summary>
    /// The Awake of the server
    /// </summary>
    private void Awake()
    {
        x_ObjectHelper = GetComponent<x_ObjectHelper>();
    }

    /// <summary>
    /// This method uses a "list" as we cant serialize Dictionaries with Unity's JsonUtility so we download the server_vars.json->transform it to list->transform to dictionary 
    /// </summary>
    /// <param name="fake_list">The fake_list<see cref="List{var_data}"/></param>
    public void write_values(List<var_data> fake_list)
    {
        dict_vars = fake_list.ToDictionary(p => p.ID);
        foreach (KeyValuePair<var_names, var_data> kvp in dict_vars)
        {
            Debug.LogErrorFormat(
                "Key {0}: val:{1} desc:{2}",
                kvp.Key,
                kvp.Value.value,
                kvp.Value.description);
        }
    }

    /// <summary>
    /// The download_server_variables, happens after server checks if its a test server of if its a live server. Note test and live use different server_vars.json from different branches in github
    /// </summary>
    /// <returns>The <see cref="IEnumerator"/></returns>
    public IEnumerator download_server_variables()
    {
        UnityEngine.Networking.UnityWebRequest uwr = UnityEngine.Networking.UnityWebRequest.Get(x_ObjectHelper.variables_repo);
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError || uwr.isHttpError)
        {
            Debug.LogError(uwr.error);
            Application.Quit();
        }
        else
        {
            try
            {
                write_values(JsonHelper.FromJson<var_data>(uwr.downloadHandler.text).ToList());
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.ToString());
                Application.Quit();
                throw;
            }
        }
    }

    /// <summary>
    /// Defines the <see cref="JsonHelper" />
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// The FromJson
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json">The json<see cref="string"/></param>
        /// <returns>The <see cref="T[]"/></returns>
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        /// <summary>
        /// The ToJson
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array<see cref="T[]"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        /// <summary>
        /// The ToJson
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array<see cref="T[]"/></param>
        /// <param name="prettyPrint">The prettyPrint<see cref="bool"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        /// <summary>
        /// Defines the <see cref="Wrapper{T}" />
        /// </summary>
        /// <typeparam name="T"></typeparam>
        [Serializable]
        private class Wrapper<T>
        {
            /// <summary>
            /// Defines the Items
            /// </summary>
            public T[] Items;
        }
    }
}
