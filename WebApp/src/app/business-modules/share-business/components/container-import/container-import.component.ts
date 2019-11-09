import { Component, OnInit, ViewChild } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { finalize, catchError } from 'rxjs/operators';
import { UploadAlertComponent } from 'src/app/shared/common/popup/upload-alert/upload-alert.component';

@Component({
    selector: 'app-container-import',
    templateUrl: './container-import.component.html'
})
export class ShareContainerImportComponent extends PopupBase implements OnInit {
    @ViewChild(UploadAlertComponent, { static: false }) importAlert: UploadAlertComponent;
    isShowInvalid: boolean = true;
    totalValidRow: number = 0;
    totalInvalidRow: number = 0;
    headers: CommonInterface.IHeaderTable[];
    data: any[];
    existedError: string = null;
    duplicatedError: string = null;
    importedData: any[] = [];
    inHouseBill: boolean = false;
    parentId: string = '';

    constructor(private _docRepo: DocumentationRepo,
        private _progressService: NgProgress) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.headers = [
            { title: 'Cont Type *', field: 'containerTypeName', sortable: true },
            { title: 'Cont Qty *', field: 'quantity', sortable: true },
            { title: 'Cont No', field: 'containerNo', sortable: true },
            { title: 'Seal No', field: 'sealNo', sortable: true },
            { title: 'GW', field: 'gw', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true },
            { title: 'NW', field: 'nw', sortable: true },
            { title: 'Package Qty', field: 'packageQuantity', sortable: true },
            { title: 'Package Type', field: 'packageTypeName', sortable: true },
            { title: 'Mark No', field: 'markNo', sortable: true },
            { title: 'Description', field: 'description', sortable: true },
            { title: 'Commodity', field: 'commodityName', sortable: true },
            { title: 'Unit', field: 'unitOfMeasureName', sortable: true },
        ];
    }

    chooseFile(file: Event) {
        if (file.target['files'] == null) { return; }
        this._progressRef.start();
        this._docRepo.upLoadContainerFile(file.target['files'])
            .pipe(
                finalize(() => {
                    this._progressRef.complete();
                    if (this.data != null) {
                        this.getData();
                    } else {
                        this.reset();
                    }
                })
            )
            .subscribe((response: any) => {
                if (response["list"] != null) {
                    this.data = response["list"];
                    this.totalItems = this.data.length;
                    this.totalValidRow = response['totalValidRows'];
                    this.duplicatedError = response['duplicatedError'];
                    this.existedError = response['existedError'];
                    this.totalInvalidRow = this.totalItems - this.totalValidRow;
                }
            }, err => {
            });
    }
    getData() {
        this.totalItems = this.data.length;
        this.page = 0;
        const end = this.page + (this.pageSize - 1);
        this.importedData = this.data.slice(this.page, end);
        console.log(this.importedData);
    }
    downloadFile() {
        this._docRepo.downloadcontainerfileExcel()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, "application/ms-excel", "ContainerImportTemplate.xlsx");
                },
            );
    }

    hideInvalid() {
        this.page = 0;
        const end = this.page + (this.pageSize - 1);
        if (this.data == null) { return; }
        this.isShowInvalid = !this.isShowInvalid;
        if (this.isShowInvalid) {
            this.totalItems = this.data.length;
            this.importedData = this.data.slice(this.page, end);
        } else {
            const inValidItems = this.data.filter(x => !x.isValid);
            this.totalItems = inValidItems.length;
            this.importedData = inValidItems.slice(this.page, end);
        }
    }

    import() {
        console.log(this.inHouseBill);
        console.log(this.parentId);
        if (this.data == null) { return; }
        if (this.totalInvalidRow > 0) {
            this.importAlert.show();
        } else {
            this._progressRef.start();
            this.data.forEach(x => {
                if (this.inHouseBill === true) {
                    x.hblid = this.parentId;
                } else { x.mblid = this.parentId; }
            });
            this._docRepo.importContainerExcel(this.data)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: any) => {
                        if (res.success) {
                            this.reset();
                            this._progressRef.complete();
                            // this.isImportSuccess.emit(true);
                            this.hide();
                        }
                        console.log(res);
                    },
                    (errors: any) => { },
                    () => { }
                );
        }
    }
    reset() {
        this.data = null;
        this.importedData = [];
        this.totalItems = 0;
        this.totalValidRow = 0;
        this.totalInvalidRow = 0;
        this.duplicatedError = null;
        this.existedError = null;
    }
    close() {
        this.reset();
        this.hide();
    }
}
