import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { ButtonModalSetting } from '../../../shared/models/layout/button-modal-setting.model';
import { ButtonType } from '../../../shared/enums/type-button.enum';
import { SystemRepo } from 'src/app/shared/repositories';
import { catchError } from 'rxjs/internal/operators/catchError';
import { finalize } from 'rxjs/internal/operators/finalize';
import { Office } from 'src/app/shared/models/system/office';
import { map } from 'rxjs/internal/operators/map';
import { NgProgress } from '@ngx-progressbar/core';
@Component({
    selector: 'app-office',
    templateUrl: './office.component.html'
})
export class OfficeComponent extends AppList implements OnInit {
    headers: CommonInterface.IHeaderTable[];
    offices: Office[] = [];

    importButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.import
    };
    exportButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.export
    };
    saveButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.save
    };

    cancelButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.cancel
    };
    addButtonSetting: ButtonModalSetting = {
        dataTarget: "add-ware-house-modal",
        typeButton: ButtonType.add
    };
    constructor(
        private _systemRepo: SystemRepo,
        private _progressService: NgProgress,

    ) {
        super();
        this._progressRef = this._progressService.ref();

        this.requestList = this.searchOffice;
    }


    ngOnInit() {
        this.headers = [
            { title: 'Office Code', field: 'code', sortable: true },
            { title: 'Name EN', field: 'branchName_EN', sortable: true },
            { title: 'Name Local', field: 'branchName_VN', sortable: true },
            { title: 'Name Abbr', field: 'shortName', sortable: true },
            { title: 'Address EN', field: 'address_EN', sortable: true },
            { title: 'Address Local', field: 'address_VN', sortable: true },
            { title: 'TaxCode', field: 'taxcode', sortable: true },
            { title: 'Company', field: 'companyName', sortable: true },
            { title: 'Status', field: 'active', sortable: true },

        ];
        this.dataSearch = {
            type: 'All'
        };
        this.searchOffice(this.dataSearch);

    }

    onSearchOffice(dataSearch: any) {
        this.dataSearch = dataSearch;
        this.searchOffice(this.dataSearch);
    }

    searchOffice(dataSearch?: any) {
        this.isLoading = true;
        this._progressRef.start();
        this._systemRepo.getOffice(this.page, this.pageSize, Object.assign({}, dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
                map((data: any) => {
                    return {
                        data: data.data.map((item: any) => new Office(item)),
                        totalItems: data.totalItems,
                    };
                })
            ).subscribe(
                (res: any) => {
                    this.totalItems = res.totalItems || 0;
                    this.offices = res.data;
                    console.log(this.offices);
                },
            );
    }


}
