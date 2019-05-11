import { Component, OnInit, Input, EventEmitter, Output } from '@angular/core';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { ButtonModalSetting } from '../../models/layout/button-modal-setting.model';
import { ColumnSetting } from '../../models/layout/column-setting.model';

@Component({
  selector: 'app-table-detail',
  templateUrl: './table-detail.component.html'
})
export class TableDetailComponent implements OnInit {
  detailButtonSetting: ButtonModalSetting = {
    dataTarget:  "",
    typeButton: ButtonType.detail
  };
  @Input() records: any[];
  @Input() settings: ColumnSetting[];
  @Input() nameDetailModal: string;
  @Input() hasAction: boolean = true;
  @Input() hasOreder: boolean = false;
  @Input() keySort: string = "";
  @Output() sortChange: EventEmitter<number> = new EventEmitter<number>();
  @Output() detail = new EventEmitter<any>();
  columnMaps: ColumnSetting[]; 
  isDesc: boolean = true;
  columnColspan = 0;
  
  constructor() { }

  ngOnInit() {
  }
  ngOnChanges(): void {
    if (this.settings) { // when settings provided
      this.columnMaps = this.settings;
      this.detailButtonSetting.dataTarget = this.nameDetailModal;
      this.columnColspan = this.columnMaps.filter(x => x.isShow == true).length + 1;
    } else { // no settings, create column maps with defaults
        this.columnMaps = Object.keys(this.records[0])
            .map( key => {
                return {
                    primaryKey: key,
                    header: key.slice(0, 1).toUpperCase() + 
                        key.replace(/_/g, ' ' ).slice(1)
            }
        });
        console.log(this.columnMaps);
    }
  }
  sort(column){
    this.keySort = column.primaryKey;
    this.isDesc = !this.isDesc;
    this.sortChange.emit(column);
  }
  detailClick(item){
    this.detail.emit(item);
  }
}
