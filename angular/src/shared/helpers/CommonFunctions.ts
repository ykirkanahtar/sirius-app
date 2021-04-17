import { Injectable } from "@angular/core";
import { AppComponentBase } from "@shared/app-component-base";
import * as moment from "moment";

@Injectable()
export class CommonFunctions {
  static dateToString(date: Date): string {
    let day = date.getDate().toString();
    let month = (date.getMonth() + 1).toString();
    let year = date.getFullYear().toString();

    if (day.length === 1) {
      day = "0" + day;
    }

    if (month.length === 1) {
      month = "0" + month;
    }

    return year + month + day;
  }

  static stringToDate(date: string): Date {
    let year = parseInt(date.substr(0, 4));
    let month = parseInt(date.substr(4, 2)) - 1;
    let day = parseInt(date.substr(6, 2));

    return new Date(year, month, day);
  }

  static toMoment(date: Date): moment.Moment {
    if (date === null) {
      throw new Error("date cannot be null");
    }

    var month = date.getMonth() + 1; //months from 1-12
    var day = date.getDate();
    var year = date.getFullYear();

    return moment({ year: year, month: month - 1, day: day });
  }

  static capitalize(data: string): string {
    return data.charAt(0).toUpperCase() + data.slice(1);
  }

  static get_header_row(sheet, xlsx, base: AppComponentBase) {
    var range = xlsx.utils.decode_range(sheet["!ref"]);
    var C,
      R = range.s.r; /* start in the first row */
    /* walk every column in the range */
    for (C = range.s.c; C <= range.e.c; ++C) {
      var cell =
        sheet[
          xlsx.utils.encode_cell({ c: C, r: R })
        ]; /* find the cell in the first row */

      let capitalizinString = CommonFunctions.capitalize(cell.v);
      let locallizeString = base.l(capitalizinString);
      cell.v = locallizeString;
    }
  }

  static createExcelFile(data: any, xlsx, baseComponent:AppComponentBase, fileName: string) {
    let worksheet = xlsx.utils.json_to_sheet(data);
    this.get_header_row(worksheet, xlsx, baseComponent);

    const workbook = {
      Sheets: { data: worksheet },
      SheetNames: ["data"],
    };
    const excelBuffer: any = xlsx.write(workbook, {
      bookType: "xlsx",
      type: "array",
    });
    this.saveAsExcelFile(excelBuffer, fileName);
  }

  static saveAsExcelFile(buffer: any, fileName: string): void {
    import("file-saver").then((FileSaver) => {
      let EXCEL_TYPE =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8";
      let EXCEL_EXTENSION = ".xlsx";
      const data: Blob = new Blob([buffer], {
        type: EXCEL_TYPE,
      });
      FileSaver.saveAs(
        data,
        fileName + "_export_" + new Date().getTime() + EXCEL_EXTENSION
      );
    });
  }
}
