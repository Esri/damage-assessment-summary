/*
 * Copyright 2015 Esri
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *   http://www.apache.org/licenses/LICENSE-2.0

 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
define([
  "dojo/_base/declare",
  "dojo/_base/lang",
  "dojo/has",
  "dojo/dom-construct",
  "dgrid/OnDemandList",
  "dgrid/Selection",
  "dijit/_WidgetBase",
  "dijit/_TemplatedMixin",
  "dijit/form/Button",
  "dojo/dom-class",
  "esri/opsdashboard/WidgetProxy",
  "esri/geometry/Extent",
  "dojo/store/Memory",
  "dojo/query",
  "dojo/store/Observable",
  "esri/tasks/query",
  "dgrid/OnDemandGrid",
  "dojo/text!./SummaryReportWidgetTemplate.html"
], function (declare, lang, has, domConstruct, List, Selection, _WidgetBase, _TemplatedMixin, Button, domClass, WidgetProxy, Extent, Memory, query, Observable, Query, Grid, templateString) {

  return declare("SummaryReportWidget", [_WidgetBase, _TemplatedMixin, WidgetProxy], {
    templateString: templateString,
    debugName: "SummaryReportWidget",

    //TODO
    // handle zoom to point...right now it just pans
    // handle user entered values into the txt boxes or change them to labels
    // handle the issue that causes the row to collapse when you zoom or pan to a feature...also check this in the native app side
    // figure out how to get the style details for when the user changes themes...can't...Jay C logged a bug
    //dataSourceExpired...look there for additional notes on this issue


    hostReady: function () {
      // Create the store we will use to display the features in the grid
      this.store = new Observable(new Memory());

      // Get from the data source and the associated data source config
      // The dataSourceConfig stores the fields selected by the operation view publisher during configuration
      var dataSourceProxy = this.dataSourceProxies[0];
      var dataSourceConfig = this.getDataSourceConfig(dataSourceProxy);

      this.hasData = false;

      this.getMapWidgetProxies().then(lang.hitch(this, function (results) {
        this._initQuery(dataSourceConfig, dataSourceProxy);
        this._createList(dataSourceConfig, dataSourceProxy, results[0]);
      }));
    },

    _initQuery: function (dataSourceConfig, dataSourceProxy) {
      var configDetails = dataSourceConfig.selectedFieldsNames;
      var oidName = dataSourceProxy.objectIdFieldName;
      var displayAlias = dataSourceConfig.displayAlias;

      //stores actual field name
      this.fieldsToQuery = [];
      //lookup object {fieldName: displayName}
      this.configFields = {};
      var idx = 0;
      for (var i = 0; i < configDetails.length; i++) {
        var f = configDetails[i];
        if (f.checked) {
          console.log("Adding Field: " + f.name);
          this.fieldsToQuery.splice(idx, 0, f.name);
          this.configFields[f.name] = displayAlias ? f.displayName : f.name;
          idx += 1;
        }
      }

      if (idx !== 0) {
        idx += 1;
      }

      if (this.fieldsToQuery.indexOf(oidName) === -1) {
        this.fieldsToQuery.splice(idx, 0, oidName);
      }

      console.log("Setting query Fields:");
      console.log(this.fieldsToQuery);
      console.log("Config Fields:");
      console.log(this.configFields);
      this.query = new Query();
      this.query.outFields = this.fieldsToQuery;
      this.query.returnGeometry = true;
    },

    _createList: function (dataSourceConfig, dataSourceProxy, mapProxy) {
      this.list = new (declare([List, Selection]))({
        mapProxy: mapProxy,
        dataProxy: dataSourceProxy,
        store: this.store,
        fields: this.fieldsToQuery,
        configFields: this.configFields,
        cleanEmptyObservers: false,
        selectionMode: this.isNative ? "extended" : "toggle",
        renderRow: function (feature) {
          var divNode = domConstruct.create('div', {
            className: "bottomBorder"
          });

          var titleDiv = domConstruct.create('div', {
            className: "titleDiv",
            onclick: function (evt) {
              var parent = evt.target.parentNode;
              if (!domClass.contains(parent, "bottomBorder")) {
                for (var i = 0; i < 2; i++) {
                  parent = parent.parentNode;
                  if (domClass.contains(parent, "bottomBorder")) {
                    break;
                  }
                }
              }
              var img = parent.childNodes[0].childNodes[0];
              var row = parent.childNodes[1];
              if (domClass.contains(row, "rowOff")) {
                domClass.remove(row, "rowOff");
                domClass.add(row, "rowOn");
                domClass.remove(img, "downImage");
                domClass.add(img, "upImage");
                domClass.remove(img, "image-down-highlight");
                domClass.add(img, "image-up-highlight");
              } else {
                domClass.remove(row, "rowOn");
                domClass.add(row, "rowOff");
                domClass.remove(img, "upImage");
                domClass.add(img, "downImage");
                domClass.remove(img, "image-up-highlight");
                domClass.add(img, "image-down-highlight");
              }
            }
          }, divNode);

          domConstruct.create('div', {
            className: "baseImage downImage image-down-highlight"
          }, titleDiv);

          var title = domConstruct.create('div', {
            className: "title"
          }, titleDiv);

          var contentDiv = domConstruct.create('div', {
            className: "rowOff",
            id: feature.id
          }, divNode);

          var idx = 0;
          for (var i = 0; i < this.fields.length; i++) {
            var fieldName = this.fields[i];
            //do this so we can have the OID in the query to support selection
            // but avoid drawing in the widget
            if (typeof (this.configFields[fieldName]) !== 'undefined') {
              domConstruct.create('label', {
                className: "fieldItemLabel",
                innerHTML: this.configFields[fieldName] + ":"
              }, contentDiv);
              domConstruct.create('input', {
                className: "fieldItemValue",
                value: feature.attributes[fieldName],
                oninput: lang.hitch(this, function (e) {
                  var v = e.srcElement.value;
                  var lbl = e.srcElement.previousElementSibling;
                  var name = lbl.textContent.substring(0, lbl.textContent.length - 1);

                  //TODO these feel sloppy...
                  var fieldName;
                  for (var i in this.configFields) {
                    if (this.configFields[i] === name) {
                      fieldName = i;
                      break;
                    }
                  }

                  var idx;
                  for (var k in this.selection) {
                    idx = k;
                    break;
                  }

                  if (typeof (this.localUpdates) === 'undefined') {
                    this.localUpdates = {};
                  }

                  this.localUpdates[fieldName + "-_-" + idx] = v;

                  //this would update the store and cause the control to re-draw...shifted to the
                  // static localUpdatesList
                  //var item = this.store.get(parseInt(idx));
                  //for (var ii in item.attributes) {
                  //  if (ii === fieldName) {
                  //    item.attributes[ii] = v;
                  //    this.store.put(item, {overwrite: true});
                  //  }
                  //}
                })
              }, contentDiv);

              if (idx === 0) {
                title.innerHTML = feature.attributes[fieldName];
                idx += 1;
              }
            }
          }

          var alignContainer = domConstruct.create('div', {
            className: "fieldItemLabel"
          }, contentDiv);

          var btnContainer = domConstruct.create('div', {
            className: "btnParent"
          }, alignContainer);

          domConstruct.create('button', {
            className: "my-btn",
            innerHTML: "Zoom To Feature",
            onclick: lang.hitch(this, function (evt) {
              var row = evt.target.parentElement.parentElement.parentElement;
              var rowData = this.row(row.id).data;
              if (rowData) {
                var geom = rowData.geometry;
                if (geom.type === "point") {
                  this.mapProxy.panTo(geom);
                } else {
                  this.mapProxy.setExtent(geom);
                }
              }
            })
          }, btnContainer);

          if (this.dataProxy.supportsSelection) {
            domConstruct.create('button', {
              className: "my-btn",
              innerHTML: "Select Feature",
              onclick: lang.hitch(this, function (evt) {
                var row = evt.target.parentElement.parentElement.parentElement;
                this.dataProxy.selectFeatures(row.id);
              })
            }, btnContainer);
          }

          return divNode;
        }
      }, this.listDiv);

      this.list.startup();
    },

    dataSourceExpired: function (dataSourceProxy, dataSourceConfig) {

      //TODO extend this to support a compare
      // rather than a blind update regardless

      //TODO will still need to decide what the expectation is if a user has modified a 
      // value in one of the txt boxes and the same value is modified on the service
      // itself...who wins...I would think we would just want to bring over the new value 
      // from the service

      console.log("dataSourceExpired");

      // Execute the query. A request will be sent to the server to query for the features.
      // The results are in the featureSet

      if (!this.hasData) {
        dataSourceProxy.executeQuery(this.query).then(function (featureSet) {
          if (this.store.data.length > 0) {
            this.store.query().forEach(function (item) {
              this.store.remove(item.id);
            }.bind(this));
          }

          if (featureSet.features) {
            featureSet.features.forEach(function (feature) {
              this.store.put(feature, {
                overwrite: true,
                id: feature.attributes[dataSourceProxy.objectIdFieldName]
              });
            }.bind(this));

            this.hasData = true;
          }
        }.bind(this));
      }
    },

    _exportCSV: function () {
      if (this.store.data.length > 0) {

        var localUpdates = this.list.localUpdates;
        var checkForMods = false;
        var oid;
        if (typeof (localUpdates) !== 'undefined') {
          checkForMods = true;
          oid = this.dataSourceProxies[0].objectIdFieldName;
        }

        var csvData = "";
        var attributes;
        var attribute;
        var line = "";
        var hasColumnNames = false;
        this.store.query().forEach(lang.hitch(this, function (item) {
          attributes = item.attributes ? item.attributes : item;

          // build the header
          if (!hasColumnNames) {
            for (attribute in attributes) {
              var configField = this.configFields[attribute];
              if (typeof (configField) !== 'undefined') {
                csvData += (csvData.length === 0 ? "" : ",") + '"' + configField + '"';
              }
            }
            csvData += "\r\n";
            hasColumnNames = true;
          }

          //populate the columns
          line = "";
          for (attribute in attributes) {
            var configField = this.configFields[attribute];
            if (typeof (configField) !== 'undefined') {
              var val = attributes[attribute];
              if (checkForMods) {
                var checkKey = attribute + "-_-" + attributes[oid];
                if (typeof (localUpdates[checkKey]) !== 'undefined') {
                  val = localUpdates[checkKey];
                }
              }

              line += (line.length === 0 ? "" : ",") + '"' + val + '"';
            }
          }
          csvData += line + "\r\n";
        }));
        //fileName - for download
        var filename = this.dataSourceProxies[0].name + ".csv";

        // native - open the data in excel file straight
        if (this.isNative) {
          csvData = "sep=,\r\n" + csvData;
          window.open("data:application/vnd.ms-excel;charset=utf-8," + encodeURIComponent(csvData), filename);
          return;
        }
        //web - the file will be downloaded to the default download location
        // Handle chrome
        if (has("chrome")) {
          var link = domConstruct.create("a", {
            href: 'data:attachment/csv;charset=utf-8,' + encodeURIComponent(csvData),
            download: filename
          }, window.document.body);

          var clickEvent = window.document.createEvent("MouseEvents");
          clickEvent.initEvent("click", true /* bubble */, true /* cancelable */);
          link.dispatchEvent(clickEvent);
          return;
        }

        //handle Firefox
        if (has("ff")) {
          var link = domConstruct.create("a", {
            href: 'data:attachment/csv;charset=utf-8,' + encodeURIComponent(csvData),
            download: filename
          }, window.parent.document.body);

          var clickEvent = window.document.createEvent("MouseEvents");
          clickEvent.initEvent("click", true /* bubble */, true /* cancelable */);
          link.dispatchEvent(clickEvent);
          return;
        }

        //handle Microsoft Internet Explorer
        var blob = new Blob([csvData], { "type": "text/csv;charset=utf8" });
        window.navigator.msSaveOrOpenBlob(blob, filename);
      }
      return;
    }
  });
});