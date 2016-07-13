﻿#region license
// Transformalize
// A Configurable ETL Solution Specializing in Incremental Denormalization.
// Copyright 2013 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion
namespace Pipeline.DotNetFiddle.Impl {
    public static class Settings {
        public static string ShortHand => @"<cfg>

  <signatures>
    <add name='none' />
    <add name='format'>
      <parameters>
        <add name='format' />
      </parameters>
    </add>
    <add name='length'>
      <parameters>
        <add name='length' />
      </parameters>
    </add>
    <add name='separator'>
      <parameters>
        <add name='separator' />
      </parameters>
    </add>
    <add name='padding'>
      <parameters>
        <add name='total-width' />
        <add name='padding-char' value='0' />
      </parameters>
    </add>
    <add name='timezone'>
      <parameters>
        <add name='from-time-zone' />
        <add name='to-time-zone' />
      </parameters>
    </add>
    <add name='fromtimezone'>
      <parameters>
        <add name='from-time-zone' value='UTC' />
      </parameters>
    </add>
    <add name='value'>
      <parameters>
        <add name='value' value='[default]' />
      </parameters>
    </add>
    <add name='type'>
      <parameters>
        <add name='type' value='[default]' />
      </parameters>
    </add>
    <add name='trim'>
      <parameters>
        <add name='trim-chars' value=' ' />
      </parameters>
    </add>
    <add name='script'>
      <parameters>
        <add name='script'  />
      </parameters>
    </add>
    <add name='map'>
      <parameters>
        <add name='map' />
      </parameters>
    </add>
    <add name='dayofweek'>
      <parameters>
        <add name='dayofweek' />
      </parameters>
    </add>
    <add name='substring'>
      <parameters>
        <add name='startindex' />
        <add name='length' value='0' />
      </parameters>
    </add>
    <add name='timecomponent'>
      <parameters>
        <add name='timecomponent' />
      </parameters>
    </add>
    <add name='replace'>
      <parameters>
        <add name='oldvalue' />
        <add name='newvalue' value='' />
      </parameters>
    </add>
    <add name='regexreplace'>
      <parameters>
        <add name='pattern' />
        <add name='newvalue' />
        <add name='count' value='0' />
      </parameters>
    </add>
    <add name='insert'>
      <parameters>
        <add name='startindex' />
        <add name='value' />
      </parameters>
    </add>
    <add name='remove'>
      <parameters>
        <add name='startindex' />
        <add name='count' value='0' />
      </parameters>
    </add>
    <add name='razor'>
      <parameters>
        <add name='template' />
        <add name='contenttype' value='raw' />
      </parameters>
    </add>
    <add name='any'>
      <parameters>
        <add name='value'/>
        <add name='operator' value='equal' />
      </parameters>
    </add>
    <add name='property'>
      <parameters>
        <add name='name' />
        <add name='property' />
      </parameters>
    </add>
    <add name='file'>
      <parameters>
        <add name='extension' value='true'/>
      </parameters>
    </add>
    <add name='xpath'>
      <parameters>
        <add name='xpath' />
        <add name='namespace' value='' />
        <add name='url' value='' />
      </parameters>
    </add>
    <add name='datediff'>
      <parameters>
        <add name='timecomponent' />
        <add name='fromtimezone' value='UTC' />
      </parameters>
    </add>
  </signatures>

  <targets>
    <add name='t' collection='transforms' property='method' />
    <add name='ignore' collection='' property='' />
  </targets>

  <methods>
    <add name='add' signature='none' target='t' />
    <add name='any' signature='any' target='t' />
    <add name='concat' signature='none' target='t' />
    <add name='connection' signature='property' target='t' />
    <add name='contains' signature='value' target='t' />
    <add name='convert' signature='type' target='t' />
    <add name='copy' signature='none' target='ignore' />
    <add name='cs' signature='script' target='t' />
    <add name='csharp' signature='script' target='t' />
    <add name='datediff' signature='datediff' target='t' />
    <add name='datepart' signature='timecomponent' target='t' />
    <add name='decompress' signature='none' target='t' />
    <add name='equal' signature='value' target='t' />
    <add name='equals' signature='value' target='t' />
    <add name='fileext' signature='none' target='t' />
    <add name='filename' signature='file' target='t' />
    <add name='filepath' signature='file' target='t' />
    <add name='format' signature='format' target='t' />
    <add name='formatphone' signature='none' target='t' />
    <add name='hashcode' signature='none' target='t' />
    <add name='htmldecode' signature='none' target='t' />
    <add name='insert' signature='insert' target='t' />
    <add name='is' signature='type' target='t' />
    <add name='javascript' signature='script' target='t' />
    <add name='join' signature='separator' target='t' />
    <add name='js' signature='script' target='t' />
    <add name='last' signature='dayofweek' target='t' />
    <add name='left' signature='length' target='t' />
    <add name='lower' signature='none' target='t' />
    <add name='map' signature='map' target='t' />
    <add name='multiply' signature='none' target='t' />
    <add name='next' signature='dayofweek' target='t' />
    <add name='now' signature='none' target='t' />
    <add name='padleft' signature='padding' target='t' />
    <add name='padright' signature='padding' target='t' />
    <add name='razor' signature='razor' target='t' />
    <add name='regexreplace' signature='regexreplace' target='t' />
    <add name='remove' signature='remove' target='t' />
    <add name='replace' signature='replace' target='t' />
    <add name='right' signature='length' target='t' />
    <add name='splitlength' signature='separator' target='t' />
    <add name='substring' signature='substring' target='t' />
    <add name='sum' signature='none' target='t' />
    <add name='timeago' signature='fromtimezone' target='t' />
    <add name='timeahead' signature='fromtimezone' target='t' />
    <add name='timezone' signature='timezone' target='t' />
    <add name='tolower' signature='none' target='t' />
    <add name='tostring' signature='format' target='t' />
    <add name='totime' signature='timecomponent' target='t' />
    <add name='toupper' signature='none' target='t' />
    <add name='toyesno' signature='none' target='t' />
    <add name='trim' signature='trim' target='t' />
    <add name='trimend' signature='trim' target='t' />
    <add name='trimstart' signature='trim' target='t' />
    <add name='upper' signature='none' target='t' />
    <add name='utcnow' signature='none' target='t' />
    <add name='xmldecode' signature='none' target='t' />
    <add name='xpath' signature='xpath' target='t' />
  </methods>

</cfg>";
    }
}
