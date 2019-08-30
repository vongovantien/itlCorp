import { Component, OnInit } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { OperationRepo } from 'src/app/shared/repositories';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'app-container-import',
  templateUrl: './container-import.component.html'
})
export class ContainerImportComponent extends PopupBase implements OnInit {

  constructor(private operationRepo: OperationRepo) {
    super();
  }

  ngOnInit() {
  }
  close() {
    this.hide();
  }
  downloadFile() {
    this.operationRepo.downloadfileExel("ContainerImportTemplate.xlsx")
      .pipe(catchError(this.catchError))
      .subscribe(
        (res: any) => {
          console.log(res);
        },
        (errors: any) => { },
        () => { }
      );
  }
}
