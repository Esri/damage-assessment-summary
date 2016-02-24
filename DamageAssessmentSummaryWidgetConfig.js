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
  'dojo/dom-construct',
  "dojo/store/Memory",
  'dojo/dom-class',
  "dijit/form/CheckBox",
  "dgrid/OnDemandList",
  "dgrid/Selection",
  "dijit/_WidgetBase",
  "dijit/_TemplatedMixin",
  "dijit/_WidgetsInTemplateMixin",
  "dojox/form/CheckedMultiSelect",
  "esri/opsdashboard/WidgetConfigurationProxy",
  "dojo/text!./DamageAssessmentSummaryWidgetConfigTemplate.html",
  "dojo/parser"
], function (declare, lang, domConstruct, Memory, domClass, CheckBox, List, Selection, _WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin, CheckedMultiSelect, WidgetConfigurationProxy, templateString) {

  return declare("DamageAssessmentSummaryWidgetConfig", [_WidgetBase, _TemplatedMixin, _WidgetsInTemplateMixin, WidgetConfigurationProxy], {
    templateString: templateString,

    postCreate: function () {
      this.inherited(arguments);

      //this.multiSelectDiv.set("labelAttr", "alias");
      //this.multiSelectDiv.set("multiple", true);
    },

    dataSourceSelectionChanged: function (dataSource, dataSourceConfig) {
      this.dataSourceConfig = dataSourceConfig;

      //var alphaNumericFields = [];
      //dataSource.fields.forEach(function (field) {
      //  switch (field.type) {
      //    case "esriFieldTypeString":
      //    case "esriFieldTypeSmallInteger":
      //    case "esriFieldTypeInteger":
      //    case "esriFieldTypeSingle":
      //    case "esriFieldTypeDouble":
      //      alphaNumericFields.push(field);
      //      return;
      //  }
      //});

      //var alphaNumericFieldsStore = new Memory({
      //  idProperty: "name",
      //  data: alphaNumericFields
      //});

      //this._createList(alphaNumericFieldsStore);
      this.dataSourceConfig.selectedFieldsNames = this._createSimpleTable(dataSource)
      //this.multiSelectDiv.set("store", alphaNumericFieldsStore);

      // Set previous fields saved in config
      //if (Array.isArray(dataSourceConfig.selectedFieldsNames))
      //  this.multiSelectDiv.set("value", dataSourceConfig.selectedFieldsNames);
    },

    _createSimpleTable: function (dataSource) {
      var table = domConstruct.create('table', {
        className: "tableTest"
      }, this.configListDiv);

      var idx = 0;
      //var header = table.createTHead();
      //var row = header.insertRow(idx);

      //this._insertCell(row, "Display", 0);
      //this._insertCell(row, "Field Alias", 1);
      //this._insertCell(row, "Field Name", 2);

      //I Guess I will load from the config if it's defiend
      //and remove from this copy if it's found...so I know what ones are left
      console.log("S");
      var dsFields = lang.clone(dataSource.fields);
      console.log(dsFields);
      console.log(dsFields.length);
      fieldLoop:
        for (var k in dsFields) {
          var f = dsFields[k];
          console.log(f);

          if (f.type !== "esriFieldTypeString" &&
              f.type !== "esriFieldTypeSmallInteger" &&
              f.type !== "esriFieldTypeInteger" &&
              f.type !== "esriFieldTypeSingle" &&
              f.type !== "esriFieldTypeDouble") {
            dsFields.splice(i, 1);
          }
        }

      var currentItems = [];
      console.log("DS");
      console.log(this.dataSourceConfig.selectedFieldsNames);
      if (this.dataSourceConfig.selectedFieldsNames) {
        for (var key in this.dataSourceConfig.selectedFieldsNames) {
          var persistField = this.dataSourceConfig.selectedFieldsNames[key];
          console.log("persistField");
          row = table.insertRow(idx);
          console.log("row");
          row.myIndex = idx;

          var checked = persistField.checked;
          console.log("checked");
          var displayName = persistField.displayName;
          console.log("displayName");
          var name = persistField.name;
          console.log("name");

          currentItems.splice(idx, 0, {
            checked: checked,
            displayName: displayName,
            name: name,
            indexInTable: idx
          });

          configFieldLoop:
            for (var i = 0; i < dsFields.length; i++) {
              if (dsFields[i].name === name) {
                dsFields.splice(i, 1);
                console.log("remove: " + name);
                break configFieldLoop;
              }
            }

          this._insertCell(row, checked, 0);
          this._insertCell(row, displayName, 1);
          this._insertCell(row, name, 2);
          idx += 1;
        }
      } else {
        dataSource.fields.forEach(lang.hitch(this, function (field) {
          row = table.insertRow(idx);
          row.myIndex = idx;
          switch (field.type) {
            case "esriFieldTypeString":
            case "esriFieldTypeSmallInteger":
            case "esriFieldTypeInteger":
            case "esriFieldTypeSingle":
            case "esriFieldTypeDouble":
              //these will be set differently if they are currently in the config
              var checked = false;
              var displayName = field.alias;

              var c = this.dataSourceConfig.selectedFieldsNames;

              currentItems.splice(idx, 0, {
                checked: checked,
                displayName: displayName,
                name: field.name,
                indexInTable: idx
              });

              this._insertCell(row, checked, 0);
              this._insertCell(row, displayName, 1);
              this._insertCell(row, field.name, 2);
              return;
          }
          idx += 1;
        }));
      }

      return currentItems;
    },

    _insertCell: function(row, v, idx){
      var cell = row.insertCell(idx);
      if (idx === 0) {
        domConstruct.create('input', {
          type: "checkbox",
          checked: v,
          onclick: lang.hitch(this, function (b) {
            if (!this.dataSourceConfig)
              return;

            var persistedNames = this.dataSourceConfig.selectedFieldsNames;

            var row = b.target.parentElement.parentElement;

            //build the object we will store here
            var fieldName = row.cells[2].childNodes[0].textContent;

            for (var i = 0; i < persistedNames.length; i++) {
              if (persistedNames[i].name === fieldName) {
                persistedNames.splice(i, 1);
                break;
              }
            }

            //TODO make sure the row index starts at 1 rather than 0 like the array...otherwise
            // remove the -1 for  the index
            persistedNames.splice(row.myIndex -1, 0, {
              checked: row.cells[0].childNodes[0].checked,
              displayName: row.cells[1].childNodes[0].value,
              name: fieldName,
              indexInTable: row.myIndex
            });

            this.readyToPersistConfig(Array.isArray(persistedNames) && persistedNames.length > 0);
          })
        }, cell);
      } else if (idx === 1) {
        domConstruct.create('input', {
          value: v
        }, cell);
      } else if (idx === 2) {
        var l = domConstruct.create('label', {
          innerHTML: v
        }, cell);
      }
    },

    _updateList: function(row){

    },

    _createList: function (alphaNumericFieldsStore) {
      //Thought about setting up the field name list like this but not sure how to move rows up and down...
      // I guess you would alter the order in the store and then update the list??

      this.list = new (declare([List, Selection]))({
        store: alphaNumericFieldsStore,
        cleanEmptyObservers: false,
        selectionMode: this.isNative ? "extended" : "toggle",
        renderRow: function (attributes) {
          var divNode = domConstruct.create('div', {
            className: "test"
          });

          return divNode;
        }
      }, this.configListDiv);

      this.list.startup();
    },

    onSelectionChanged: function (value) {
      if (!this.dataSourceConfig)
        return;

      this.dataSourceConfig.selectedFieldsNames = value;
      this.readyToPersistConfig(Array.isArray(value) && value.length > 0);
    },

    rowClicked: function (evt) {
      var t = evt.target.nextElementSibling;
      if (domClass.contains(t, "rowOff")) {
        domClass.remove(t, "rowOff");
        domClass.add(t, "rowOn");
      } else {
        domClass.remove(t, "rowOn");
        domClass.add(t, "rowOff");
      }
    }
  });
});
















