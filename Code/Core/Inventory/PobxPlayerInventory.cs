using Sandbox;
using Conna.Inventory;

//public sealed class PobxPlayerInventory : BaseInventory
//{
//	public PobxPlayerInventory( Guid id ) : base( id, 5, 5 ) // 5 columns × 5 rows
//	{

//	}
//}

public class PobxPlayerInventory( Guid id, int width, int height, InventorySlotMode slotMode = InventorySlotMode.Single ) : BaseInventory( id, width, height, slotMode )
{
	/// <summary>
	/// When it is fully dropped. OnItemRemoved is used for other actions as wel. This one is drop and remove only.
	/// </summary>
	public Action<PobxBaseInventoryItem> OnItemFullyDroppedFromInventory;



	public VacuumGun VacuumGun { get; set; }
	public EnergyPistolWeapon PistolWeapon { get; set; }
	public M4A4Weapon AssaultRifleWeapon { get; set; }


	public Weapon ActiveWeapon { get; set; }
	public GadgetBase Gadget { get; set; }

	public InventoryResult RemoveItemAndDropFromInventory(PobxBaseInventoryItem item)
	{
		var result = TryRemove( item );

		if (result == InventoryResult.Success)
			OnItemFullyDroppedFromInventory?.Invoke(item);

		return result;
	}

	public bool TryFindPlacement( InventoryItem item, out InventorySlot slot )
	{
		var effectiveW = GetEffectiveWidth( item );
		var effectiveH = GetEffectiveHeight( item );

		for ( var y = 0; y <= Height - effectiveH; y++ )
		{
			for ( var x = 0; x <= Width - effectiveW; x++ )
			{
				if ( !IsRectFree( x, y, effectiveW, effectiveH ) || !CanPlaceAt( item, x, y, effectiveW, effectiveH ) )
					continue;

				slot = new InventorySlot( x, y, effectiveW, effectiveH );
				return true;
			}
		}

		slot = default;
		return false;
	}

	public bool IsRectFree( int x, int y, int w, int h )
	{
		for ( var row = y; row < y + h; row++ )
			if ( RowIntersects( row, x, w ) ) return false;
		return true;
	}

	public bool RowIntersects( int row, int x, int w )
	{
		var endBit = x + w;
		var startChunk = x / 64;
		var endChunk = (endBit - 1) / 64;
		var rowBase = RowOffset( row );

		for ( var chunk = startChunk; chunk <= endChunk; chunk++ )
		{
			var chunkStart = chunk * 64;
			var lo = Math.Max( x, chunkStart ) - chunkStart;
			var hi = Math.Min( endBit, chunkStart + 64 ) - chunkStart;
			var mask = (hi - lo) == 64 ? ulong.MaxValue : ((1UL << (hi - lo)) - 1UL) << lo;
			if ( (RowBits[rowBase + chunk] & mask) != 0 ) return true;
		}
		return false;
	}

	public int RowOffset( int y ) => y * ChunksPerRow;
}
