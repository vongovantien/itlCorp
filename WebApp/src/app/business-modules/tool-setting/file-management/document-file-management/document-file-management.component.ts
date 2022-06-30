import { Component, OnInit } from '@angular/core';
import { AppList } from '@app';
import { SystemFileManageRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'document-file-management',
  templateUrl: './document-file-management.component.html'
})
export class DocumentFileManagementComponent extends AppList implements OnInit {

  constructor(
    private fileRepo:SystemFileManageRepo,
    private readonly _toastService: ToastrService,
  ) { 
    super();
  }

  ngOnInit() {
  }

  deleteFileManagerment(contractId: string, fileName: string) {
    this.fileRepo.deleteContractFilesAttach(contractId, fileName)
      .pipe(catchError(this.catchError))
      .subscribe(
        (res: any) => {
          this._toastService.warning(res.message);
        },
      );
  }

}
