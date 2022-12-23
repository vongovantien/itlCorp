import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { ConfirmPopupComponent } from '@common';
import { SystemConstants } from '@constants';
import { InjectViewContainerRefDirective } from '@directives';
import { ExportRepo, SystemFileManageRepo } from '@repositories';
import { AppList } from 'src/app/app.list';
import { fileManagePaging } from '../../general-file-management/general-file-management.component';
import { SortService } from './../../../../../shared/services/sort.service';

@Component({
    selector: 'app-list-file-management',
    templateUrl: './list-file-management.component.html'
})
export class ListFileManagementComponent extends AppList implements OnInit {

    @Output() changePage: EventEmitter<any> = new EventEmitter<any>();
    @Input() tabType: string;
    @Input() listEdocFile: fileManagePaging;
    @Input() headers: CommonInterface.IHeaderTable[];
    @ViewChild(ConfirmPopupComponent) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild(InjectViewContainerRefDirective) viewContainer: InjectViewContainerRefDirective;
    edocId: string = '';
    selectedFile: any = null;
    isView: boolean = true;

    constructor(
        private _systemFileRepo: SystemFileManageRepo,
        private readonly _exportRepo: ExportRepo,
        private _sortService: SortService
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
        if (this.tabType === 'Acc') {
            this.headers.splice(0, 0, { field: 'acRefNo', title: 'A/C Ref No', sortable: true },)
            this.headers.join();
        }
    }

    viewEdocFromName(edocFile: any) {
        this.selectedFile = edocFile;
        this.viewFileEdoc();
    }

    viewFileEdoc() {
        if (!this.selectedFile.imageUrl) {
            return;
        }
        const extension = this.selectedFile.imageUrl.split('.').pop();
        if (['xlsx', 'docx', 'doc', 'xls'].includes(extension.toLowerCase())) {
            this._exportRepo.previewExport(this.selectedFile.imageUrl);
        }
        else if (['html', 'htm'].includes(extension.toLowerCase())) {
            console.log();
            this._systemFileRepo.getFileEdocHtml(this.selectedFile.imageUrl).subscribe(
                (res: any) => {
                    window.open('', '_blank').document.write(res.body);
                }
            )
        }
        else if (['pdf', 'txt', 'png', 'jpeg'].includes(extension.toLowerCase())) {
            this._exportRepo.downloadExport(this.selectedFile.imageUrl);
        } else {
            this.downloadEdoc();
        }
    }

    sortFile(sortField: string, order: boolean) {
        this.listEdocFile.data = this._sortService.sort(this.listEdocFile.data, sortField, order);
    }


    updatePagingEdocFile(e: { page: number, pageSize: number, data: any }) {
        let pageData: any = {
            page: e.page,
            pageSize: e.pageSize
        };
        this.changePage.emit(pageData);
    }

    onSelectFile(edoc: any) {
        this.selectedFile = edoc;
        this.isView = true;
        const extension = this.selectedFile.imageUrl.split('.').pop();
        if (!['xlsx', 'docx', 'doc', 'xls', 'html', 'htm', 'pdf', 'txt', 'png', 'jpeg'].includes(extension.toLowerCase())) {
            this.isView = false;
        }
    }

    downloadEdoc() {
        const selectedFile = Object.assign({}, this.selectedFile);
        if (selectedFile.id === selectedFile.sysImageId) {
            this._systemFileRepo.getFileEdoc(selectedFile.sysImageId).subscribe(
                (data) => {
                    const extention = selectedFile.userFileName.split('.').pop();
                    this.downLoadFile(data, SystemConstants.FILE_EXCEL, selectedFile.systemFileName + '.' + extention);
                }
            )
        } else {
            this._systemFileRepo.getFileEdoc(selectedFile.sysImageId).subscribe(
                (data) => {
                    const extention = selectedFile.userFileName.split('.').pop();
                    this.downLoadFile(data, SystemConstants.FILE_EXCEL, selectedFile.systemFileName + '.' + extention);
                }
            )
        }
    }
}
