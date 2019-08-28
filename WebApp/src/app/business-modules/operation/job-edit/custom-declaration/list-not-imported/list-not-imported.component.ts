import { Component, OnInit, Input } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { SortService } from 'src/app/shared/services';

@Component({
    selector: 'app-list-not-imported',
    templateUrl: './list-not-imported.component.html'
})
export class ListNotImportedComponent extends AppList implements OnInit {
    @Input() notImportedData: any[] = [];
    notImportedCustomClearances: any[];
    checkAllNotImported = false;
    headers: CommonInterface.IHeaderTable[];

    constructor(
        private _sortService: SortService) {
        super();
        this.requestSort = this.sortLocal;
        this.requestList = this.getDataNotImported;
    }

    ngOnInit() {
        this.totalItems = this.notImportedData.length;
        this.notImportedCustomClearances = this.notImportedData.slice(this.page - 1, this.pageSize);
    }

    ngOnChanges() {
        this.headers = [
            { title: 'Custom No', field: 'clearanceNo', sortable: true },
            { title: 'Import Date', field: 'datetimeCreated', sortable: true },
            { title: 'Clearance Date', field: 'clearanceDate', sortable: true },
            { title: 'HBL No', field: 'hblid', sortable: true },
            { title: 'Export Country', field: 'exportCountryCode', sortable: true },
            { title: 'Import Country', field: 'importCountryCode', sortable: true },
            { title: 'Commodity Code', field: 'commodityCode', sortable: true },
            { title: 'Qty', field: 'qtyCont', sortable: true },
            { title: 'Parentdoc', field: 'firstClearanceNo', sortable: true },
            { title: 'Note', field: 'note', sortable: true },
        ];
    }
    sortLocal(sort: string): void {
        this.notImportedCustomClearances = this._sortService.sort(this.notImportedCustomClearances, sort, this.order);
    }
    changeAllNotImported() { }
    removeAllChecked() { }
    getDataNotImported() {
        if (this.notImportedData != null) {
            this.totalItems = this.notImportedData.length;
            console.log(this.notImportedData);
            const end = (this.page - 1) + this.pageSize;
            this.notImportedCustomClearances = this.notImportedData.slice(this.page - 1, end);
        }
    }
}
