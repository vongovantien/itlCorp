import { ToastrService } from 'ngx-toastr';
import { Component, OnInit, ChangeDetectionStrategy, Input } from '@angular/core';
import { AppForm } from '@app';
import { AccountingRepo } from '@repositories';

@Component({
    selector: 'settlement-attach-file-list',
    templateUrl: './attach-file-list-settlement.component.html',
    styleUrls: ['./attach-file-settlement.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class SettlementAttachFileListComponent extends AppForm implements OnInit {
    @Input() Id: string;

    files: any[] = [];
    selectedFile: any;

    constructor(
        private _accountingRepo: AccountingRepo,
        private _toastService: ToastrService
    ) {
        super();
    }

    ngOnInit() { }

    chooseFile(_$event: any) {
        const fileList: FileList[] = event.target['files'];
        if (fileList.length > 0 && !!this.Id) {
            this._accountingRepo.uploadFileSettlement(this.Id, fileList)
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success("Upload file successfully!");
                        }
                    }
                );
        }
    }

    deleteFile(_file: any) {

    }

}