import { Component, Input, OnInit, QueryList, ViewChildren } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ContextMenuDirective } from '@directives';
import { Store } from '@ngrx/store';
import { DocumentationRepo, ExportRepo, SystemFileManageRepo } from '@repositories';
import { SortService } from '@services';
import { IAppState, getCurrentUserState } from '@store';
import { ToastrService } from 'ngx-toastr';
import { takeUntil } from 'rxjs/operators';
import { getListEdocState } from 'src/app/business-modules/accounting/settlement-payment/components/store';
import { AppShareEDocBase } from '../edoc.base';


@Component({
    selector: 'list-file-attach',
    templateUrl: './list-file-attach.component.html',
})
export class ShareListFilesAttachComponent extends AppShareEDocBase implements OnInit {

    @ViewChildren(ContextMenuDirective) queryListMenuContext: QueryList<ContextMenuDirective>;

    @Input() transactionType: string = '';
    @Input() readonly: boolean = false;
    @Input() keyword: string;


    constructor(
        protected readonly _systemFileRepo: SystemFileManageRepo,
        protected readonly _activedRoute: ActivatedRoute,
        protected readonly _store: Store<IAppState>,
        protected readonly _toast: ToastrService,
        protected readonly _exportRepo: ExportRepo,
        protected readonly _sortService: SortService,
        protected _documentationRepo: DocumentationRepo,
    ) {
        super(_toast, _systemFileRepo, _exportRepo, _store);
    }

    ngOnInit(): void {
        this._store.select(getCurrentUserState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res) => {
                    this.currentUser = res;
                }
            )
        if (this.transactionType !== 'Shipment') {
            if (this.transactionType === "Settlement") {
                //this.requestListEDocSettle();
                this.filterJob(null);
            }
            else {
                this.getEDoc(this.transactionType);
            }
        }
    }

    filterJob(jobNo: string) {
        this._store.select(getListEdocState).pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res: any) => {
                    if (this.jobOnSettle && jobNo !== null) {
                        console.log(this.jobOnSettle, this.jobNo);
                        this.lstEdocExist = res.filter(x => x.jobNo === jobNo || x.jobNo === null);
                    } else {
                        this.lstEdocExist = res;
                    }
                }
            );

    }

    onSelectEDoc(edoc: any) {
        this.selectedEdoc = edoc;
        this.selectedEdoc1 = edoc;
        this.isView = true;
        const extension = this.selectedEdoc.imageUrl.split('.').pop();
        if (!['xlsx', 'docx', 'doc', 'xls', 'html', 'htm', 'pdf', 'txt', 'png', 'jpeg', 'jpg'].includes(extension)) {
            this.isView = false;
        }
        this.clearMenuContext(this.queryListMenuContext);
    }
}
