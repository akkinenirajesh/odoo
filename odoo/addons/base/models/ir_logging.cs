C#
public partial class BaseIrLogging {
    // all the model methods are written here.
    public void Init() {
        // super(IrLogging, self).init()
        // self._cr.execute("select 1 from information_schema.constraint_column_usage where table_name = 'ir_logging' and constraint_name = 'ir_logging_write_uid_fkey'")
        // if self._cr.rowcount:
        //     # DROP CONSTRAINT unconditionally takes an ACCESS EXCLUSIVE lock
        //     # on the table, even "IF EXISTS" is set and not matching; disabling
        //     # the relevant trigger instead acquires SHARE ROW EXCLUSIVE, which
        //     # still conflicts with the ROW EXCLUSIVE needed for an insert
        //     self._cr.execute("ALTER TABLE ir_logging DROP CONSTRAINT ir_logging_write_uid_fkey")
    }
}
