import { Component, OnInit, OnChanges, Input, Output, EventEmitter } from '@angular/core';
import { ColumnSetting } from '../../models/layout/column-setting.model';
import { ButtonModalSetting } from '../../models/layout/button-modal-setting.model';
import { ButtonType } from '../../enums/type-button.enum';

@Component({
    selector: 'app-table-layout',
    templateUrl: './table-layout.component.html'
})
export class TableLayoutComponent implements OnInit, OnChanges {
    @Input() records: any[];
    @Input() caption: string;
    @Input() settings: ColumnSetting[];
    @Input() nameEditModal: string;
    @Input() keySort: string = "";
    @Input() id: string = "";

    @Output() sortChange: EventEmitter<number> = new EventEmitter<number>();
    @Output() edit = new EventEmitter<any>();
    @Output() delete = new EventEmitter<any>();
    @Output() save = new EventEmitter<any>();

    columnMaps: ColumnSetting[];
    isDesc: boolean = true;
    columnColspan = 0;

    editButtonSetting: ButtonModalSetting = {
        dataTarget: "",
        typeButton: ButtonType.edit
    };
    deleteButtonSetting: ButtonModalSetting = {

        dataTarget: "confirm-delete-modal",
        typeButton: ButtonType.delete
    };

    ngOnInit() {
    }

    ngOnChanges(): void {
        if (this.settings) { // when settings provided
            this.columnMaps = this.settings;
            this.editButtonSetting.dataTarget = this.nameEditModal;
            this.columnColspan = this.columnMaps.filter(x => x.isShow == true).length + 1;
        } else { // no settings, create column maps with defaults
            if (this.records == undefined) { return; }
            this.columnMaps = Object.keys(this.records[0])
                .map(key => {
                    return {
                        primaryKey: key,
                        header: key.slice(0, 1).toUpperCase() +
                            key.replace(/_/g, ' ').slice(1)
                    };
                });
            console.log(this.columnMaps);
        }
    }
    sort(column: any) {
        this.keySort = column.primaryKey;
        this.isDesc = !this.isDesc;
        this.sortChange.emit(column);
    }

    editClick(item: any) {
        this.edit.emit(item);
    }

    deleteClick(item: any) {
        this.delete.emit(item);
    }
}
