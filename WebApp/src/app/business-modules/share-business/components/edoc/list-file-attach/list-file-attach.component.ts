import { Component, Input, OnInit, QueryList, ViewChildren } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ContextMenuDirective } from '@directives';
import { Store } from '@ngrx/store';
import { DocumentationRepo, ExportRepo, SystemFileManageRepo } from '@repositories';
import { SortService } from '@services';
import { IAppState, getCurrentUserState } from '@store';
import { ToastrService } from 'ngx-toastr';
import { catchError, takeUntil } from 'rxjs/operators';
import { AppShareEDocBase } from '../edoc.base';


@Component({
    selector: 'list-file-attach',
    templateUrl: './list-file-attach.component.html',
})
export class ShareBussinessListFilesAttachComponent extends AppShareEDocBase implements OnInit {

    @Input() lstEdocExist: any;
    @Input() jobBase: string = '';
    @ViewChildren(ContextMenuDirective) queryListMenuContext: QueryList<ContextMenuDirective>;

    constructor(
        protected readonly _systemFileRepo: SystemFileManageRepo,
        protected readonly _activedRoute: ActivatedRoute,
        protected readonly _store: Store<IAppState>,
        protected readonly _toast: ToastrService,
        protected readonly _exportRepo: ExportRepo,
        protected readonly _sortService: SortService,
        protected _documentationRepo: DocumentationRepo,
    ) {
        super(_toast, _systemFileRepo, _exportRepo);
    }

    ngOnInit(): void {
        this._store.select(getCurrentUserState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res) => {
                    this.currentUser = res;
                }
            )
        this.getListEdocExist();
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
        console.log(this.selectedEdoc);
    }

    getListEdocExist() {
        this._systemFileRepo.getEDocByAccountant(this.billingId, 'Settlement')
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any) => {
                    let data = res;
                    data.eDocs = res.eDocs.filter(x => x.jobNo === this.jobNo || x.jobNo === null);
                    this.lstEdocExist = data;
                    if (res?.eDocs.length > 0) {
                        this.isEdocByAcc = true;
                    }
                },
            );
    }

}
