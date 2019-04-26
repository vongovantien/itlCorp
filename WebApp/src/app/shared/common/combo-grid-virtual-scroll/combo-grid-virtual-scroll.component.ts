import { Component, OnInit, Input, Output, EventEmitter, OnChanges, SimpleChange, SimpleChanges, ChangeDetectorRef, AfterViewInit } from '@angular/core';
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
export class ComboGridVirtualScrollComponent implements OnInit, OnChanges,AfterViewInit {

  currentItemSelected: any = null;
  CurrentActiveItemIdObj: { field: string, value: any, hardValue: any } = null;
  indexSelected: number = -1;
  displaySelectedStr: string = '';


  ConstDataSources: any[] = [];
  DataSources: any[] = [];
  DisplayFields: { field: string, label: string }[] = [];
  SearchFields: string[] = [];
  SelectedDisplayFields: string[] = [];
  CurrentActiveItem: any = null;

  @Input() dataSources: any[];
  @Input() displayFields: { field: string, label: string }[];
  @Input() searchFields: any[];
  @Input() selectedDisplayFields: any[];
  @Input() currentActiveItemId: any;


  ngOnChanges(changes: SimpleChanges): void {
    const _dataSources: SimpleChange = changes.dataSources;
    const _displayFields: SimpleChange = changes.displayFields;
    const _selectedDisplayFields: SimpleChange = changes.selectedDisplayFields;
    const _currentActiveItemId: SimpleChange = changes.currentActiveItemId;

    if (_dataSources !== undefined && _dataSources !== null) {
      this.setDataSource(_dataSources.currentValue);
    }
    if (_displayFields !== undefined && _displayFields !== null) {
      this.setDisplayFields(_displayFields.currentValue);
    }
    if (_selectedDisplayFields !== undefined && _selectedDisplayFields !== null) {
      this.setSelectedDisplayFields(_selectedDisplayFields.currentValue);
    }
    if (_currentActiveItemId !== undefined && _currentActiveItemId !== null) {
      this.setCurrentActiveItemId(_currentActiveItemId.currentValue);
    }

  }

  private setDataSource(data: any[]) {
    if (data != undefined && data.length > 0) {
      this.DataSources = data;
      this.ConstDataSources = cloneDeep(data);

      if (this.CurrentActiveItemIdObj !== null) {
        var activeItemData = this.CurrentActiveItemIdObj;
        var itemIndex = findIndex(this.ConstDataSources, function (o) {
          return o[activeItemData.field] === activeItemData.value;
        });
        if (itemIndex != -1) {
          this.indexSelected = itemIndex;
          var item = this.ConstDataSources[itemIndex];
          this.setCurrentActiveItem(item);
        }
        if (itemIndex === -1 && activeItemData.hardValue != null) {
          this.displaySelectedStr = activeItemData.hardValue;
        }
      }
    }
  }

  private setDisplayFields(data: { field: string, label: string }[]) {
    if (data.length > 0) {
      this.DisplayFields = data;
    }
  }

  private setSelectedDisplayFields(data: any[]) {
    if (data.length > 0) {
      this.SelectedDisplayFields = data;
    }
  }

  private setCurrentActiveItemId(data: any) {
    if (data.value != null) {
      this.CurrentActiveItemIdObj = data;
      var itemIndex = findIndex(this.ConstDataSources, function (o) {
        return o[data.field] === data.value;
      });
      if (itemIndex != -1) {
        this.indexSelected = itemIndex;
        var item = this.ConstDataSources[itemIndex];
        this.setCurrentActiveItem(item);
      }
      if (itemIndex === -1 && data.hardValue != null) {
        this.displaySelectedStr = data.hardValue;
      }
    }
  }

  /**
   * OUTPUT DATA
   */
  @Output() itemSelected = new EventEmitter<any>();

  constructor(private cdr: ChangeDetectorRef) { }

  ngAfterViewInit(): void {
    console.log("VIEW INITED ")
    this.cdr.markForCheck();
  }


  ngOnInit() {

  }

  emitSelected(item: any) {
    this.itemSelected.emit(item);
    this.setCurrentActiveItem(item);
    this.currentItemSelected = item;
    if (this.CurrentActiveItemIdObj !== null && this.CurrentActiveItemIdObj.value !== null) {
      this.CurrentActiveItemIdObj.value = item[this.CurrentActiveItemIdObj.field];
    }
  }

  setCurrentActiveItem(item: any) {
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
        const value: string = o[field] == null ? "" : o[field].toString();
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

    console.log(this.DataSources);

    if (this.CurrentActiveItemIdObj !== null && this.CurrentActiveItemIdObj.value !== null) {
      var _CurrentActiveItemIdObj: { field: string, value: any, hardValue: any } = this.CurrentActiveItemIdObj;
      this.indexSelected = findIndex(this.DataSources, function (o) {
        return o[_CurrentActiveItemIdObj.field] === _CurrentActiveItemIdObj.value;
      });
    }


  }


  /**
   * Auto focus to search input whenever user open combo grid 
   */
  opening(event: any) {
    var srcEle = $(event.srcElement);
    var targetEle = srcEle.closest('div.dropdown').find('input.cbgr-input-search');
    setTimeout(() => {
      targetEle.focus();
    }, 100);
  }





}
