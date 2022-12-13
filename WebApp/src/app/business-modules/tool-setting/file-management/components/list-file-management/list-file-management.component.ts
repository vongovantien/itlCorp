import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { ConfirmPopupComponent } from '@common';
import { SystemConstants } from '@constants';
import { SystemFileManageRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { fileManagePaging } from '../../general-file-management/general-file-management.component';

@Component({
    selector: 'app-list-file-management',
    templateUrl: './list-file-management.component.html'
})
export class ListFileManagementComponent extends AppList implements OnInit {

    @Input() listEdocFile: fileManagePaging;
    @Input() headers: CommonInterface.IHeaderTable[];
    edocId: string = '';
    @ViewChild(ConfirmPopupComponent) confirmDeletePopup: ConfirmPopupComponent;
    @Output() reGetEdoc: EventEmitter<boolean> = new EventEmitter<false>();
    @Output() changePage: EventEmitter<any> = new EventEmitter<any>();

    constructor(
        private _systemFileRepo: SystemFileManageRepo,
        private _toast: ToastrService,
    ) {
        super();
    }

    ngOnInit() {
        this.headers = [
            { field: 'systemFileName', title: 'Alias Name', sortable: true },
            { field: 'documentType', title: 'Document Type', sortable: true },
            { field: 'jobRef', title: 'Job Ref', sortable: true },
            { field: 'hblNo', title: 'House Bill No', sortable: true },
            { field: 'billingNo', title: 'Billing No', sortable: true },
            { field: 'source', title: 'Source', sortable: true },
            { field: 'type', title: 'Tag', sortable: true },
            { field: 'userFileName', title: 'Real File Name', sortable: true },
            { field: 'datetimeCreated', title: 'Attach Time', sortable: true },
            { field: 'userCreated', title: 'Attach Person', sortable: true },
        ]
    }

    onDeleteEdoc(id: string = '') {
        this.edocId = id;
        this.confirmDeletePopup.show();
    }

    deleteEdoc() {
        this._systemFileRepo.deleteEdoc(this.edocId)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any) => {
                    if (res.status) {
                        this._toast.success("Delete Sucess");
                        this.confirmDeletePopup.close();
                        this.reGetEdoc.emit(true);
                    }
                },
            );
    }

    updatePagingEdocFile(e: { page: number, pageSize: number, data: any }) {
        let pageData: any = {
            page: e.page,
            pageSize: e.pageSize
        };
        this.changePage.emit(pageData);
    }

    downloadEdoc(edoc: any) {
        console.log(edoc);

        const selectedEdoc = Object.assign({}, edoc);
        if (selectedEdoc.id === selectedEdoc.sysImageId) {
            this._systemFileRepo.getFileEdoc(selectedEdoc.sysImageId).subscribe(
                (data) => {
                    const extention = selectedEdoc.userFileName.split('.').pop();
                    this.downLoadFile(data, SystemConstants.FILE_EXCEL, selectedEdoc.systemFileName + '.' + extention);
                }
            )
        } else {
            this._systemFileRepo.getFileEdoc(selectedEdoc.sysImageId).subscribe(
                (data) => {
                    const extention = selectedEdoc.userFileName.split('.').pop();
                    this.downLoadFile(data, SystemConstants.FILE_EXCEL, selectedEdoc.systemFileName + '.' + extention);
                }
            )
        }
    }
}
