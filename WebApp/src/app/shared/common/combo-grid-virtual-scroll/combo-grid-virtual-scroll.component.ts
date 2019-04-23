import { Component, OnInit, Input, Output, EventEmitter, AfterViewInit } from '@angular/core';
import filter from 'lodash/filter';
import cloneDeep from 'lodash/cloneDeep';
import findIndex from 'lodash/findIndex';
import $ from 'jquery';
// declare var $: any;
@Component({
  selector: 'app-combo-grid-virtual-scroll',
  templateUrl: './combo-grid-virtual-scroll.component.html',
  styleUrls: ['./combo-grid-virtual-scroll.component.scss']
})
export class ComboGridVirtualScrollComponent implements OnInit {

  currentItemSelected:any = null;
  CurrentActiveItemIdObj:{field:string,value:any} = null;
  indexSelected: number = -1;
  displaySelectedStr: string = '';


  ConstDataSources: any[] = [];
  DataSources: any[] = [];
  DisplayFields: { field: string, label: string }[] = [];
  SearchFields: string[] = [];
  SelectedDisplayFields: string[] = [];
  CurrentActiveItem:any = null;

  /**
   * INPUT DATA
   */
  @Input() set dataSources(data: any[]) {
    if (data.length > 0) {
      this.DataSources = data;
      this.ConstDataSources = cloneDeep(data);
    }
  }

  @Input() set displayFields(data: { field: string, label: string }[]) {
    if (data.length > 0) {
      this.DisplayFields = data;
    }
  }

  @Input() set searchFields(data: any[]) {
    if (data.length > 0) {
      this.SearchFields = data;
    }
  }

  @Input() set selectedDisplayFields(data: any[]) {
    if (data.length > 0) {
      this.SelectedDisplayFields = data;
    }
  }

  @Input() set currentActiveItemId(data: any) {
    if (data.value !=null) {
      this.CurrentActiveItemIdObj = data;
      var itemIndex = findIndex(this.ConstDataSources,function(o){
        console.log(o[data.field]);
        return o[data.field] === data.value;
      });
      if(itemIndex!=-1){
        this.indexSelected = itemIndex;
        var item = this.ConstDataSources[itemIndex];
        this.setCurrentActiveItem(item);
      }
    }
  }



  /**
   * OUTPUT DATA
   */
  @Output() itemSelected = new EventEmitter<any>();

  constructor() { }


  ngOnInit() {

  }

  emitSelected(item: any) {
    this.itemSelected.emit(item);
    this.setCurrentActiveItem(item);
    this.currentItemSelected = item;
    this.CurrentActiveItemIdObj.value = item[this.CurrentActiveItemIdObj.field];
  }

  setCurrentActiveItem(item:any){
    this.displaySelectedStr = '';
    for (var i = 0; i < this.SelectedDisplayFields.length; i++) {
      const field = this.SelectedDisplayFields[i];

      if (i === this.SelectedDisplayFields.length - 1) {
        this.displaySelectedStr += item[field];
      }
      else {
        this.displaySelectedStr += item[field] + " - ";
      }

    }
  }

  Search(key: string) {
    // this.indexSelected = -1;
    // this.displaySelectedStr = '';
    key = key.toLowerCase().trim();
    var constData = this.ConstDataSources;
    var displayFields = this.DisplayFields;
    this.DataSources = filter(constData, function (o) {
      var matched: boolean = false;

      for (var i = 0; i < displayFields.length; i++) {
        const field: string = displayFields[i].field;
        const value: string = o[field]==null?"":o[field].toString();
        const valueType: string = typeof value;

        if (valueType === 'boolean' && value === key) {
          matched = true;
        }

        if (valueType !== 'boolean' && value.toLowerCase().includes(key)) {
          matched = true;
        }
      }    
      return matched;
    });

    var _CurrentActiveItemIdObj : {field:string,value:any} = this.CurrentActiveItemIdObj;
    this.indexSelected = findIndex(this.DataSources,function(o){
      return o[_CurrentActiveItemIdObj.field] === _CurrentActiveItemIdObj.value;
    });

  }


  /**
   * Auto focus to search input whenever user open combo grid 
   */
  opening(event:any){
    var srcEle = $(event.srcElement);
    var targetEle = srcEle.closest('div.dropdown').find('input.cbgr-input-search');
    setTimeout(() => {
      targetEle.focus();
    }, 100);
  }

  



}
